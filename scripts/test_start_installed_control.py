"""Installed-user launcher tests. Never start an app or alter an installed descriptor."""
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

HERE = Path(__file__).resolve().parent
CHECKS = Path(r"D:\A_KJ\AI\Workspace\ControlTower\continuous-20261007\checks")
VERSIONS = Path(r"D:\A_KJ\AI\Applications\AIControlTower\versions")
PACKAGE = "0.9.9-20261008-remote-repair"
APP = VERSIONS / PACKAGE
SCRIPT = HERE / "start_installed_control.ps1"
PS = Path(os.environ.get("WINDIR", r"C:\Windows")) / "System32/WindowsPowerShell/v1.0/powershell.exe"


class InstalledControlLauncherTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        CHECKS.mkdir(parents=True, exist_ok=True)
        cls.temp = Path(tempfile.mkdtemp(prefix="installed-launch-", dir=CHECKS)).resolve()
        cls.versions = cls.temp / "versions"
        cls.package = cls.versions / PACKAGE
        cls.package.mkdir(parents=True)
        shutil.copyfile(APP / "AIControlTower.exe", cls.package / "AIControlTower.exe")
        cls.original = json.loads((APP / "DEPLOYMENT.json").read_text(encoding="utf-8-sig"))

    @classmethod
    def tearDownClass(cls):
        if not cls.temp.is_relative_to(CHECKS.resolve()):
            raise AssertionError("fixture cleanup outside checks")
        shutil.rmtree(cls.temp)

    def setUp(self):
        self.descriptor = dict(self.original, expiresAtUtc="2000-01-01T00:00:00Z", activated=True)

    def run_guard(self, *, descriptor=None, check=True, shim="", versions=None):
        self.assertTrue(SCRIPT.is_file(), "installed-user launcher is not implemented")
        (self.package / "DEPLOYMENT.json").write_text(json.dumps(descriptor or self.descriptor), encoding="utf-8")
        # Fixture-only source copy: replace the sole fixed versions directory, not the approved identity.
        text = SCRIPT.read_text(encoding="utf-8-sig")
        self.assertEqual(1, text.count(str(VERSIONS)))
        text = text.replace(str(VERSIONS), str(versions or self.versions))
        if shim:
            text = text.replace("$probed = $false", "$probed = $false\n" + shim)
        staged = self.temp / "guard.ps1"
        staged.write_text(text, encoding="utf-8-sig")
        args = [str(PS), "-NoLogo", "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", str(staged)]
        if check:
            args.append("-CheckOnly")
        result = subprocess.run(args, capture_output=True, text=True, timeout=15)
        rows = [line for line in result.stdout.splitlines() if line.startswith("{")]
        self.assertTrue(rows, "missing bounded guard result")
        return result.returncode, json.loads(rows[-1])

    def assert_held(self, descriptor):
        code, row = self.run_guard(descriptor=descriptor)
        self.assertEqual(2, code)
        self.assertEqual("held", row["status"])
        self.assertFalse(row["started"])

    def test_expired_verified_activated_package_is_valid_check_only(self):
        code, row = self.run_guard()
        self.assertEqual(0, code)
        self.assertEqual("verified", row["status"])
        self.assertFalse(row["started"])
        self.assertFalse(row["instanceProbed"])

    def test_check_only_never_probes_or_launches(self):
        code, row = self.run_guard(shim="function Get-CimInstance { throw 'must-not-probe' }")
        self.assertEqual(0, code)
        self.assertFalse(row["instanceProbed"])

    def test_wrong_descriptor_hash_is_held(self):
        self.assert_held(dict(self.descriptor, fileSha256="f" * 64))

    def test_wrong_source_is_held(self):
        self.assert_held(dict(self.descriptor, sourceCommit="f" * 40))

    def test_unknown_package_version_is_held(self):
        self.assert_held(dict(self.descriptor, version="0.9.10"))

    def test_unactivated_or_string_activation_is_held(self):
        for activation in (False, "true"):
            with self.subTest(activation=activation):
                self.assert_held(dict(self.descriptor, activated=activation))

    def test_wrong_product_version_is_held(self):
        self.assert_held(dict(self.descriptor, productVersion="0.9.9+" + "f" * 40))

    def test_non_utc_original_deadline_metadata_is_held(self):
        self.assert_held(dict(self.descriptor, expiresAtUtc="2000-01-01T00:00:00+09:00"))

    def test_missing_activation_is_held(self):
        descriptor = dict(self.descriptor)
        del descriptor["activated"]
        self.assert_held(descriptor)

    def test_arbitrary_package_override_is_not_a_supported_parameter(self):
        result = subprocess.run(
            [str(PS), "-NoLogo", "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass",
             "-File", str(SCRIPT), "-CheckOnly", "-VersionRoot", str(self.package)],
            capture_output=True, text=True, timeout=15)
        self.assertNotEqual(0, result.returncode)
        self.assertNotIn('"status":"started"', result.stdout)

    def test_oversized_descriptor_is_held(self):
        self.assert_held(dict(self.descriptor, ignored="x" * 17000))

    def test_changed_executable_hash_is_held(self):
        exe = self.package / "AIControlTower.exe"
        before = exe.stat().st_size
        try:
            with exe.open("ab") as stream:
                stream.write(b"not-approved")
            self.assert_held(self.descriptor)
        finally:
            with exe.open("r+b") as stream:
                stream.truncate(before)

    def test_wrong_path_is_held(self):
        code, row = self.run_guard(versions=self.temp / "absent-versions")
        self.assertEqual(2, code)
        self.assertEqual("held", row["status"])

    def test_reparse_package_is_held(self):
        link_versions = self.temp / "linked-versions"
        link_versions.mkdir()
        link = link_versions / PACKAGE
        command = "New-Item -ItemType Junction -Path '" + str(link) + "' -Value '" + str(self.package) + "' | Out-Null"
        subprocess.run([str(PS), "-NoProfile", "-NonInteractive", "-Command", command], check=True, capture_output=True, timeout=10)
        try:
            code, row = self.run_guard(versions=link_versions)
            self.assertEqual(2, code)
            self.assertEqual("held", row["status"])
        finally:
            os.rmdir(link)

    def test_current_manual_instance_is_held_without_activation(self):
        shim = "function Get-CimInstance { [pscustomobject]@{ExecutablePath=(Join-Path $versions '0.9.9-20261008-remote-repair\\AIControlTower.exe');CommandLine='AIControlTower.exe --manual-control --no-activate-existing'} }"
        code, row = self.run_guard(check=False, shim=shim)
        self.assertEqual(3, code)
        self.assertEqual("manual_instance_running", row["reason"])
        self.assertFalse(row["started"])
        self.assertTrue(row["instanceProbed"])


if __name__ == "__main__":
    unittest.main()
