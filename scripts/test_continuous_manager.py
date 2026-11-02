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
        value = self.snapshot()['steps']['a']
        events = [json.loads(line) for line in (self.state / 'events.private.jsonl').read_text().splitlines()]
        event_names = [row['event'] for row in events]
        self.assertFalse((self.state / 'stop.request.json').exists())
        if value['status'] == 'blocked':
            # The 0.2s run budget includes durable prelaunch checkpoint/event I/O.
            self.assertEqual('stop requested or run watchdog expired', value['reason'])
            self.assertEqual(0, value['attempts'])
            self.assertNotIn('step_started', event_names)
            self.assertNotIn('child_started', event_names)
        else:
            self.assertEqual('timed_out', value['status'])
            self.assertEqual(1, value['attempts'])
            self.assertIn('step_started', event_names)
            self.assertEqual(1, sum(row['event'] == 'child_started' and row.get('step') == 'a' for row in events))

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

    def ai_step(self, code, contract=None):
        cli = self.root / 'fake_ai.py'
        cli.write_text(code, encoding='utf-8')
        step = {'id': 'a', 'type': 'codex', 'executable': sys.executable,
                'prefix_argv': [str(cli)], 'model': 'gpt-6.1-sol', 'prompt': 'fixture only',
                'depends_on': [], 'timeout_seconds': 5, 'cwd': '.'}
        if contract is not None:
            step['completion_contract'] = contract
        return step

    def ai_success_code(self):
        lines = [{'type': 'item.completed', 'item': {'type': 'agent_message', 'text': 'private fixture report'}},
                 {'type': 'turn.completed', 'usage': {'input_tokens': 1, 'output_tokens': 1}}]
        return '\n'.join('print(' + repr(json.dumps(line)) + ')' for line in lines)

    def test_ai_exit_zero_turn_failed_blocks_successor_and_resume(self):
        failure = json.dumps({'type': 'turn.failed', 'error': {'message': 'unsupported account'}})
        p = self.plan([self.ai_step('print(' + repr(failure) + ')'),
                       self.step('next', "from pathlib import Path; Path('must_not_run').touch()", ['a'])])
        self.assertEqual(1, self.run_plan(p))
        self.assertEqual('failed', self.snapshot()['steps']['a']['status'])
        self.assertEqual('blocked', self.snapshot()['steps']['next']['status'])
        self.assertFalse((self.root / 'must_not_run').exists())
        self.assertEqual(1, self.run_plan(p))
        self.assertEqual(1, self.snapshot()['steps']['a']['attempts'])

    def test_ai_terminal_without_report_and_stderr_success_do_not_complete(self):
        for code in ('print(\'{"type":"turn.completed"}\')',
                     'import sys; print(\'{"type":"turn.completed"}\',file=sys.stderr)',
                     'print("not json")', 'print("{}")', 'print("x"*70000)'):
            with self.subTest(code=code):
                self.state = self.root / ('state-' + str(abs(hash(code))))
                self.assertEqual(1, self.run_plan(self.plan([self.ai_step(code)])))
                self.assertEqual('output_limit' if '70000' in code else 'failed', self.snapshot()['steps']['a']['status'])

    def test_ai_failure_wins_over_valid_terminal_and_report(self):
        code = self.ai_success_code() + '\nprint(\'{"type":"error","message":"failed"}\')'
        self.assertEqual(1, self.run_plan(self.plan([self.ai_step(code)])))
        self.assertEqual('failed', self.snapshot()['steps']['a']['status'])

    def test_ai_success_receipt_binds_immutable_plan_and_never_publishes_report(self):
        p = self.plan([self.ai_step(self.ai_success_code())])
        self.assertEqual(0, self.run_plan(p))
        receipt = self.snapshot()['steps']['a'].get('aiReceipt')
        self.assertIsInstance(receipt, dict, 'declared AI requires a typed completion receipt')
        self.assertEqual(1, receipt['schemaVersion'])
        self.assertEqual('harmless/a', receipt['executionId'])
        self.assertEqual(self.cm.plan_hash(p), receipt['planSha256'])
        self.assertEqual('harmless', receipt['planId'])
        self.assertEqual('a', receipt['stepId'])
        self.assertEqual('succeeded', receipt['status'])
        self.assertTrue(receipt['terminalObserved'])
        self.assertTrue(receipt['reportObserved'])
        self.assertNotIn('private fixture report', json.dumps(receipt))

    def test_explicit_wrapper_failed_receipt_cannot_override_physical_exit_zero(self):
        wrapper = {'state': 'failed', 'exitCode': 1, 'reportPresent': False, 'model': 'gpt-6.1-sol'}
        step = self.step('a', 'print(' + repr(json.dumps(wrapper)) + ')')
        step['completion_contract'] = {'adapter': 'wrapper-json-v1', 'model': 'gpt-6.1-sol'}
        self.assertEqual(1, self.run_plan(self.plan([step])))
        self.assertEqual('failed', self.snapshot()['steps']['a']['status'])

    def test_generic_command_json_stdout_stays_opaque(self):
        p = self.plan([self.step('a', 'print(\'{"state":"failed","exitCode":1,"reportPresent":false}\')')])
        self.assertEqual(0, self.run_plan(p))
        self.assertNotIn('aiReceipt', self.snapshot()['steps']['a'])

    def test_required_report_must_be_new_nonempty_bounded_and_inside_root(self):
        report = self.root / 'report.txt'
        report.write_text('old report', encoding='utf-8')
        contract = {'adapter': 'codex-jsonl-v1', 'required_report_path': 'report.txt'}
        code = "from pathlib import Path; Path('spawned').touch()\n" + self.ai_success_code()
        p = self.plan([self.ai_step(code, contract)])
        self.assertEqual(1, self.run_plan(p))
        self.assertFalse((self.root / 'spawned').exists(), 'stale report must block before spawn')
        for path in ('../escape', str(self.root.parent / 'escape.txt'), 'Desktop/report.txt'):
            with self.subTest(path=path):
                p['steps'][0]['completion_contract']['required_report_path'] = path
                with self.assertRaises(ValueError):
                    self.cm.validate_plan(p)

    def test_required_report_new_artifact_is_hashed_not_replaced_by_stdout_claim(self):
        contract = {'adapter': 'codex-jsonl-v1', 'required_report_path': 'report.txt'}
        code = "from pathlib import Path; Path('report.txt').write_text('new private report')\n" + self.ai_success_code()
        self.assertEqual(0, self.run_plan(self.plan([self.ai_step(code, contract)])))
        receipt = self.snapshot()['steps']['a']['aiReceipt']
        self.assertTrue(receipt['artifactVerified'])
        self.assertEqual(64, len(receipt['artifactSha256']))
        self.assertNotIn('new private report', json.dumps(receipt))

    def test_ai_router_auto_and_astra_models_rejected_before_execution(self):
        p = self.plan([self.ai_step(self.ai_success_code())])
        for model in ('jev-router', 'auto', 'router', 'gpt-6-astra'):
            with self.subTest(model=model):
                p['steps'][0]['model'] = model
                with self.assertRaises(ValueError):
                    self.cm.validate_plan(p)

    def test_usage_metadata_only_allows_bounded_integer_counters(self):
        path = self.root / 'usage.log'
        path.write_text(json.dumps({'type': 'turn.completed', 'usage': {'input_tokens': 3,
                        'output_tokens': 'SECRET_PROMPT', 'raw_report': 'PRIVATE_BODY',
                        'cached_input_tokens': True}}), encoding='utf-8')
        self.assertEqual({'input_tokens': 3}, self.cm.read_usage(path))

    def test_required_report_stale_empty_and_stdout_only_claim_fail(self):
        import os
        for artifact_code in ('', "from pathlib import Path; Path('report.txt').touch()",
                              "from pathlib import Path; import os; Path('report.txt').write_text('new'); os.utime('report.txt',(1,1))"):
            with self.subTest(artifact=artifact_code):
                self.state = self.root / ('artifact-state-' + str(abs(hash(artifact_code))))
                report = self.root / 'report.txt'
                if report.exists():
                    report.unlink()
                code = artifact_code + '\n' + self.ai_success_code()
                contract = {'adapter': 'codex-jsonl-v1', 'required_report_path': 'report.txt'}
                self.assertEqual(1, self.run_plan(self.plan([self.ai_step(code, contract)])))
                self.assertFalse(self.snapshot()['steps']['a']['aiReceipt']['artifactVerified'])


class JevItemErrorCompletionTests(unittest.TestCase):
    # Reuse only fixture helpers, not the old class's tests or default temp location.
    step = ContinuousManagerTests.step
    plan = ContinuousManagerTests.plan
    run_plan = ContinuousManagerTests.run_plan
    snapshot = ContinuousManagerTests.snapshot
    ai_step = ContinuousManagerTests.ai_step
    ai_success_code = ContinuousManagerTests.ai_success_code

    def setUp(self):
        spec = importlib.util.spec_from_file_location('continuous_manager_item_errors', MODULE)
        self.cm = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.cm)
        base = self.cm.safe_path(Path('D:/A_KJ/AI/Workspace/ControlTower/continuous-20261007/checks/jev-python-item-errors'))
        base.mkdir(parents=True, exist_ok=True)
        self.temp = tempfile.TemporaryDirectory(dir=base, prefix='owned-')
        self.addCleanup(self.temp.cleanup)
        self.root = self.cm.safe_path(Path(self.temp.name), base)
        self.state = self.root / 'private'

    def assert_item_error_blocks_dependency(self, item, after_completion, event_type='item.completed'):
        event = {'type': event_type, 'item': item}
        error_code = 'print(' + repr(json.dumps(event)) + ')'
        success_code = self.ai_success_code()
        code = (success_code + '\n' + error_code if after_completion
                else error_code + '\n' + success_code)
        plan = self.plan([self.ai_step(code), self.step('next',
                         "from pathlib import Path; Path('must_not_run').touch()", ['a'])])
        self.assertEqual(1, self.run_plan(plan))
        state = self.snapshot()
        receipt = state['steps']['a']['aiReceipt']
        self.assertEqual('failed', state['steps']['a']['status'])
        self.assertEqual('failed', receipt['status'])
        self.assertEqual('ai_failed', receipt['failureCode'])
        self.assertEqual(0, receipt['processExitCode'])
        self.assertTrue(receipt['terminalObserved'])
        self.assertTrue(receipt['reportObserved'])
        self.assertEqual('blocked', state['steps']['next']['status'])
        self.assertEqual(0, state['steps']['next']['attempts'])
        self.assertFalse((self.root / 'must_not_run').exists())
        self.assertNotIn('PRIVATE_ITEM_ERROR', json.dumps(receipt))
        self.assertNotIn('private fixture report', json.dumps(receipt))
        self.assertEqual(1, self.run_plan(plan))
        replay = self.snapshot()
        self.assertEqual(1, replay['steps']['a']['attempts'])
        self.assertEqual(receipt, replay['steps']['a']['aiReceipt'])
        self.assertEqual('blocked', replay['steps']['next']['status'])
        self.assertEqual(0, replay['steps']['next']['attempts'])
        self.assertFalse((self.root / 'must_not_run').exists())

    def assert_normal_item_allows_dependency(self, item):
        code = 'print(' + repr(json.dumps({'type': 'item.completed', 'item': item})) + ')\n' + self.ai_success_code()
        plan = self.plan([self.ai_step(code), self.step('next',
                         "from pathlib import Path; Path('success.marker').touch()", ['a'])])
        self.assertEqual(0, self.run_plan(plan))
        state = self.snapshot()
        receipt = state['steps']['a']['aiReceipt']
        self.assertEqual('succeeded', receipt['status'])
        self.assertIsNone(receipt['failureCode'])
        self.assertEqual('succeeded', state['steps']['next']['status'])
        self.assertTrue((self.root / 'success.marker').exists())
        self.assertNotIn('PRIVATE_REASONING', json.dumps(receipt))
        self.assertNotIn('private fixture report', json.dumps(receipt))
        self.assertEqual(0, self.run_plan(plan))
        replay = self.snapshot()
        self.assertEqual(1, replay['steps']['a']['attempts'])
        self.assertEqual(1, replay['steps']['next']['attempts'])

    def test_error_item_before_completion_blocks_dependency(self):
        self.assert_item_error_blocks_dependency({'type': 'error', 'message': 'PRIVATE_ITEM_ERROR'}, False)

    def test_error_item_after_completion_blocks_dependency(self):
        self.assert_item_error_blocks_dependency({'type': 'error', 'message': 'PRIVATE_ITEM_ERROR'}, True)

    def test_nested_tool_error_before_completion_blocks_dependency(self):
        self.assert_item_error_blocks_dependency({'type': 'mcp_tool_call', 'status': 'failed',
                                                 'error': {'message': 'PRIVATE_ITEM_ERROR'}}, False)

    def test_nested_tool_error_after_completion_blocks_dependency(self):
        self.assert_item_error_blocks_dependency({'type': 'mcp_tool_call', 'status': 'failed',
                                                 'error': {'message': 'PRIVATE_ITEM_ERROR'}}, True)

    def test_updated_tool_error_before_completion_blocks_dependency(self):
        self.assert_item_error_blocks_dependency({'type': 'mcp_tool_call',
                                                 'error': {'message': 'PRIVATE_ITEM_ERROR'}}, False, 'item.updated')

    def test_updated_tool_error_after_completion_blocks_dependency(self):
        self.assert_item_error_blocks_dependency({'type': 'mcp_tool_call',
                                                 'error': {'message': 'PRIVATE_ITEM_ERROR'}}, True, 'item.updated')

    def test_recoverable_command_nonzero_allows_dependency(self):
        self.assert_normal_item_allows_dependency({'type': 'command_execution', 'status': 'failed', 'exit_code': 17})

    def test_null_tool_error_allows_dependency(self):
        self.assert_normal_item_allows_dependency({'type': 'mcp_tool_call', 'status': 'completed', 'error': None})

    def test_successful_tool_allows_dependency(self):
        self.assert_normal_item_allows_dependency({'type': 'command_execution', 'status': 'completed', 'exit_code': 0})

    def test_reasoning_allows_dependency(self):
        self.assert_normal_item_allows_dependency({'type': 'reasoning', 'text': 'PRIVATE_REASONING'})


if __name__ == '__main__':
    unittest.main()
