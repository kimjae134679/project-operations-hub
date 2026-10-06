"""Real, harmless child-process checks; never invoke an AI service."""
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import threading
import time
import unittest


MODULE = Path(__file__).with_name('continuous_manager.py')


class ContinuousManagerTests(unittest.TestCase):
    def setUp(self):
        self.assertTrue(MODULE.exists(), 'continuous manager implementation is missing')
        spec = importlib.util.spec_from_file_location('continuous_manager', MODULE)
        self.cm = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.cm)
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name).resolve()
        self.state = self.root / 'private'

    def step(self, name, code, depends=None, timeout=5):
        return {'id': name, 'type': 'command', 'argv': [sys.executable, '-c', code],
                'depends_on': depends or [], 'timeout_seconds': timeout, 'cwd': '.'}

    def plan(self, steps):
        return {'schema_version': 1, 'id': 'harmless', 'project': 'test',
                'approved_root': str(self.root), 'max_steps': 10,
                'max_run_seconds': 10, 'max_output_bytes': 1048576, 'steps': steps}

    def run_plan(self, plan, digest=None):
        return self.cm.run_plan(plan, self.state, digest or self.cm.plan_hash(plan))

    def snapshot(self):
        return json.loads((self.state / 'state.json').read_text(encoding='utf-8'))

    def test_two_real_processes_dependency_and_resume_without_reexecution(self):
        p = self.plan([self.step('first', "from pathlib import Path; Path('result.txt').write_text('first')"),
                       self.step('second', "from pathlib import Path; p=Path('result.txt'); p.write_text(p.read_text()+' second')", ['first'])])
        self.assertEqual(0, self.run_plan(p))
        self.assertEqual('first second', (self.root / 'result.txt').read_text())
        self.assertEqual(0, self.run_plan(p))
        self.assertEqual('first second', (self.root / 'result.txt').read_text())
        self.assertEqual('succeeded', self.snapshot()['status'])

    def test_failure_blocks_dependent_and_is_not_retried(self):
        p = self.plan([self.step('first', 'raise SystemExit(7)'),
                       self.step('second', "from pathlib import Path; Path('bad').touch()", ['first'])])
        self.assertEqual(1, self.run_plan(p))
        self.assertFalse((self.root / 'bad').exists())
        self.assertEqual('failed', self.snapshot()['steps']['first']['status'])
        self.assertEqual('blocked', self.snapshot()['steps']['second']['status'])
        self.assertEqual(1, self.run_plan(p))
        self.assertEqual(1, self.snapshot()['steps']['first']['attempts'])

    def test_out_of_order_dependency_failure_propagates_to_entire_chain(self):
        p = self.plan([self.step('c', 'print(3)', ['b']), self.step('b', 'print(2)', ['a']),
                       self.step('a', 'raise SystemExit(7)')])
        self.assertEqual(1, self.run_plan(p))
        self.assertEqual('blocked', self.snapshot()['steps']['b']['status'])
        self.assertEqual('blocked', self.snapshot()['steps']['c']['status'])

    def test_windows_atomic_checkpoint_tolerates_brief_status_reader(self):
        if sys.platform != 'win32':
            self.skipTest('Windows file-sharing semantics')
        path = self.root / 'checkpoint.json'
        self.cm.atomic_json(path, {'old': True})
        reader = path.open('rb')
        timer = threading.Timer(0.2, reader.close)
        timer.start()
        try:
            self.cm.atomic_json(path, {'new': True})
        finally:
            timer.join()
            reader.close()
        self.assertEqual({'new': True}, json.loads(path.read_text()))

    def test_wrong_approval_or_changed_plan_rejected(self):
        p = self.plan([self.step('a', 'print(1)')])
        with self.assertRaises(ValueError):
            self.run_plan(p, '0' * 64)
        self.assertEqual(0, self.run_plan(p))
        p['steps'][0]['argv'][-1] = 'print(2)'
        with self.assertRaises(ValueError):
            self.run_plan(p)

    def test_ambiguous_interrupted_step_blocked_never_retried(self):
        p = self.plan([self.step('a', "from pathlib import Path; Path('bad').touch()")])
        self.state.mkdir()
        state = self.cm.initial_state(p)
        state['steps']['a']['status'] = 'running'
        state['steps']['a']['attempts'] = 1
        self.cm.atomic_json(self.state / 'state.json', state)
        self.assertEqual(1, self.run_plan(p))
        self.assertEqual('blocked', self.snapshot()['steps']['a']['status'])
        self.assertFalse((self.root / 'bad').exists())

    def test_timeout_stops_own_process_and_dependent(self):
        p = self.plan([self.step('a', 'import time; time.sleep(30)', timeout=0.2),
                       self.step('b', "from pathlib import Path; Path('bad').touch()", ['a'])])
        self.assertEqual(1, self.run_plan(p))
        self.assertEqual('timed_out', self.snapshot()['steps']['a']['status'])
        self.assertFalse((self.root / 'bad').exists())

    def launch(self, p):
        path = self.root / 'plan.json'
        path.write_text(json.dumps(p), encoding='utf-8')
        proc = subprocess.Popen([sys.executable, str(MODULE), 'run', '--plan', str(path),
                                 '--state-dir', str(self.state), '--approve-sha256', self.cm.plan_hash(p)],
                                stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        def cleanup_process():
            if proc.poll() is None:
                proc.kill()
            proc.communicate(timeout=5)
        self.addCleanup(cleanup_process)
        deadline = time.monotonic() + 5
        while time.monotonic() < deadline:
            if (self.state / 'state.json').exists() and self.snapshot()['steps']['a']['status'] == 'running':
                return proc, path
            time.sleep(0.03)
        self.fail('child runner never reached running state')

    def test_stop_request_and_exclusive_lock_real_runner(self):
        p = self.plan([self.step('a', 'import time; time.sleep(30)', timeout=40),
                       self.step('b', "from pathlib import Path; Path('bad').touch()", ['a'])])
        p['max_run_seconds'] = 50
        proc, _ = self.launch(p)
        with self.assertRaises(ValueError):
            self.run_plan(p)
        self.cm.request_stop(self.state)
        out, err = proc.communicate(timeout=5)
        self.assertEqual(1, proc.returncode, (out, err))
        self.assertEqual('stopped', self.snapshot()['steps']['a']['status'])
        self.assertFalse((self.root / 'bad').exists())

    def test_declared_root_cwd_state_and_cycle_guards(self):
        p = self.plan([self.step('a', 'print(1)')])
        for cwd in ('../escape', str(self.root.parent), 'Desktop'):
            with self.subTest(cwd=cwd):
                p['steps'][0]['cwd'] = cwd
                with self.assertRaises(ValueError):
                    self.cm.validate_plan(p)
        p['steps'][0]['cwd'] = '.'
        p['steps'][0]['depends_on'] = ['a']
        with self.assertRaises(ValueError):
            self.cm.validate_plan(p)
        p['steps'][0]['depends_on'] = []
        with self.assertRaises(ValueError):
            self.cm.run_plan(p, self.root.parent, self.cm.plan_hash(p))

    def test_symlink_cwd_is_rejected_when_supported(self):
        link = self.root / 'linked'
        try:
            link.symlink_to(self.root, target_is_directory=True)
        except OSError:
            self.skipTest('symlink creation unavailable to this account')
        p = self.plan([self.step('a', 'print(1)')])
        p['steps'][0]['cwd'] = 'linked'
        with self.assertRaises(ValueError):
            self.cm.validate_plan(p)

    def test_agent_model_explicit_bounded_and_no_bypass_flags(self):
        p = self.plan([{'id': 'a', 'type': 'codex', 'executable': sys.executable,
                        'model': 'gpt-5.5', 'prompt': 'Read local rules, then do the scoped task.',
                        'depends_on': [], 'timeout_seconds': 5, 'cwd': '.'}])
        self.cm.validate_plan(p)
        argv, prompt = self.cm.step_command(p['steps'][0], self.root)
        self.assertIn('workspace-write', argv)
        self.assertIn('approval_policy="never"', argv)
        self.assertNotIn('--dangerously-bypass-approvals-and-sandbox', argv)
        self.assertIn('Do not touch the Desktop', prompt)
        for model in ('', 'gpt-6-astra', 'ASTRA'):
            p['steps'][0]['model'] = model
            with self.assertRaises(ValueError):
                self.cm.validate_plan(p)

    def test_private_logs_capture_usage_not_publication(self):
        event = json.dumps({'type': 'turn.completed', 'usage': {'input_tokens': 3, 'output_tokens': 2}})
        p = self.plan([self.step('a', 'print(' + repr(event) + '); import sys; print("stderr", file=sys.stderr)')])
        self.assertEqual(0, self.run_plan(p))
        self.assertIn(event, (self.state / 'logs' / 'a.stdout.private.log').read_text())
        self.assertEqual(3, self.snapshot()['steps']['a']['usage']['input_tokens'])
        self.assertEqual('stderr\n', (self.state / 'logs' / 'a.stderr.private.log').read_text())

    def test_output_limit_and_run_watchdog(self):
        p = self.plan([self.step('a', 'print("x" * 100000)')])
        p['max_output_bytes'] = 1024
        self.assertEqual(1, self.run_plan(p))
        self.assertEqual('output_limit', self.snapshot()['steps']['a']['status'])
        self.assertLessEqual(sum(f.stat().st_size for f in (self.state / 'logs').glob('*.log')), 1024)
        p2 = self.plan([self.step('a', 'import time; time.sleep(30)', timeout=40)])
        p2['id'] = 'watchdog'
        p2['max_run_seconds'] = 0.2
        self.state = self.root / 'watchdog-private'
        self.assertEqual(1, self.run_plan(p2))
        self.assertEqual('timed_out', self.snapshot()['steps']['a']['status'])

    def test_windows_job_kills_child_when_owner_crashes(self):
        if sys.platform != 'win32':
            self.skipTest('Windows job containment check')
        p = self.plan([self.step('a', "import time; from pathlib import Path; time.sleep(2); Path('escaped').touch()", timeout=10)])
        proc, _ = self.launch(p)
        # Wait for actual child ownership, not merely the pre-spawn checkpoint.
        events = self.state / 'events.private.jsonl'
        deadline = time.monotonic() + 3
        while time.monotonic() < deadline and 'child_started' not in events.read_text():
            time.sleep(0.03)
        self.assertIn('child_started', events.read_text())
        proc.kill()
        proc.communicate(timeout=5)
        time.sleep(2.2)
        self.assertFalse((self.root / 'escaped').exists(), 'owner crash leaked its mutating child')

    def test_agent_timeout_when_cli_never_reads_large_prompt(self):
        cli = self.root / 'fake_cli.py'
        cli.write_text('import time; time.sleep(30)', encoding='utf-8')
        p = self.plan([{'id': 'a', 'type': 'codex', 'executable': sys.executable,
                        'prefix_argv': [str(cli)], 'model': 'gpt-5.5', 'prompt': 'x' * 12000,
                        'depends_on': [], 'timeout_seconds': 0.2, 'cwd': '.'}])
        proc, _ = self.launch(p)
        try:
            out, err = proc.communicate(timeout=3)
        except subprocess.TimeoutExpired:
            self.fail('runner blocked writing prompt before reaching its watchdog')
        self.assertEqual(1, proc.returncode, (out, err))
        self.assertEqual('timed_out', self.snapshot()['steps']['a']['status'])


if __name__ == '__main__':
    unittest.main()
