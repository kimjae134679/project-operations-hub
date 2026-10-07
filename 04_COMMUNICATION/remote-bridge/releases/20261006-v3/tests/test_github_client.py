import io,json,sys,unittest,urllib.error
from pathlib import Path
from unittest.mock import patch
sys.path.insert(0,str(Path(__file__).parents[1]))
import bridge_worker as b
class Response:
 headers={}
 def __init__(self,value):self.value=value
 def __enter__(self):return self
 def __exit__(self,*args):pass
 def read(self,*args):return json.dumps(self.value).encode()
class GitHubTests(unittest.TestCase):
 def client(self):
  c=b.GitHub('kimjae134679/private','remote/pc-bridge');c.token='PRIVATE_AUTH';return c
 def test_metadata_url_has_no_trailing_slash_and_token_not_in_url(self):
  client=self.client()
  with patch.object(b.urllib.request,'urlopen',return_value=Response({'private':True})) as opener:
   self.assertTrue(client.request('GET','')['private'])
  request=opener.call_args.args[0]
  self.assertEqual(request.full_url,'https://api.github.com/repos/kimjae134679/private');self.assertNotIn('PRIVATE_AUTH',request.full_url)
 def test_reject_absolute_or_foreign_url(self):
  for path in ('/contents/test','https://example.com/token'):
   with self.assertRaises(b.BridgeError):self.client().request('GET',path)
 def test_auth_repo_branch_and_queue_errors_are_distinct(self):
  for code,path,expected in ((401,'','github_authentication_failed'),(403,'','github_permission_or_rate_limit'),(404,'','github_repository_not_found_or_access_denied'),(404,'branches/remote%2Fpc-bridge','github_branch_not_found_or_access_denied'),(404,'contents/_pc_bridge/queue.json','github_http_404')):
   with patch.object(b.urllib.request,'urlopen',side_effect=urllib.error.HTTPError('url',code,'error',{},io.BytesIO())):
    with self.assertRaisesRegex(b.BridgeError,expected):self.client().request('GET',path)
 def test_repository_mutations_share_lock_across_audio_and_pc_clients(self):
  self.assertIs(b.GitHub().lock,b.GitHub().lock);self.assertIsNot(b.GitHub().lock,self.client().lock)
if __name__=='__main__':unittest.main()
