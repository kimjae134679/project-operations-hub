# Threads ���� �ܺ� ���� �԰� ? ���� ������ �ʾ�

Ȯ����: 2026-10-10(KST). ����: ���� �� �̹��������� ���� �� ���� ���� �� Buffer ����� ���� ������ �״�� �̾� �а� �Ѵ�. �� � ��Ű���� ���� API�� �������� �ʴ´�. `contract.example.json`�� ���� ���Ϻ� ��¥ ���ø� �� ������ ���� ������, ��°�� � �ۿ� import�ϴ� ������ �ƴϴ�. ���� �ڷᡤ�̹��������ȡ������������� ���� �ʴ´�.

�ֽ� ����� ���� �ݿ�: **���� �Խù��� ����20���� �����ϰ�, �ø� ���� ���� �ø� �� ���̾ƿ��� �����Ѵ�.** ��� �Խ� �׽�Ʈ�� �� �÷��� ����5���� �������ϵ��� ��û�ƴ�. ������ ���� ������ ������ ���� provider ���¸� ��ȸ�ϰ� ���� �̷� �ð��� provider ID�� ������ �����ϵ��� ��û�ƴ�. ���� �ڵ�ȭ ��Ȱ��ȭ�� ���õƴ�. �� ���� �ۼ��ڴ� �Խá����� �������ڵ�ȭ ������ ���� �ʾ�����, **���ÿ� ���� ���� Ȯ���� �����Ѵ�.** ���� �ڻꡤ������ ������ ����ų� ���� ���� ������ ���� �Խø� ���� �������� �ʴ´�.

���� ���� �̷�(���� �������� ������� ����): ���� �Խÿ��� ���簢 ǥ���� ���� ������ ���� �¿� �߸��� ĳ���� ���� �۾� �߸��� �߰ߵǾ� ���� ���α׷� �켱 ��������ü �غ� �̹��� �����, �� �Խ�/���� ���� �Ͻ� ����, ���� �̰Խ� ������ ������ ������ ��û�ƾ���. �� �� **�Խá����� ���� �켱 ������ �� �ֽ� �Խ� �켱 ���÷� ��ü�ƴ�.** ���� provider ������ ����ƴ����� ���� Ȯ���� �ʿ��ϴ�.

���ۡ����䡤���� ������ �Խ� ����� ���� �帧���� ����Ѵ�. ���ο� ���� render�� ������ ��ģ �� �������� ������ �ݿ��ϸ�, ���������������̹� �Խù��� �������� �ʴ´�. ����ڰ� ������ ���̾ƿ� ���� ���� �°� ������ �����ϰ� �ǹ� ������ ������Ѵ�. ���� ���� ������ũ�⡤���� ������ ���� ����� ���� ����� �ʿ��ϴ�.

## ���� �ۼ� ������

| �ܰ� | ���� �ۼ��ڿ� ���� ������ | ���� �ܰ谡 �ϴ� �� |
|---|---|---|
| ���������� | ����/���� ���: ���� candidate ID, ������, ����ó, ��ǡ��Ǹ� �ٰ� | ���� ID�� ���� �ٰŸ� �а� �����Ѵ�. �� �ܰ迡�� ��ǡ��򰡸� �ٲ��� �ʴ´�. |
| �̹��������� ���� | ���� ���: `status.json`, �� outputFolder�� `production-plan.json`, `publish-captions.json`, rendered �̹��� | ���� ����� �а� ������ �� �ڽ��� �ڻ� ����ҷ� �����´�. ������ ���� �ʴ´�. |
| ���� ���� | ���� ��� ���� ����: `.local/state.json`, `.local/final-review-decisions.json`, immutable handoff | Buffer ����� ���� ��ȿ�� ����� �д´�. ����� ������ ��� �������� �ٲ��� �ʴ´�. |
| Buffer ��� | Buffer ��� Buffer ����: �ڱ� ����/�Խ� ��������� ���� ��� | ���� ���� �÷����� ����� �д´�. ���� ���ϡ����� ������ ��ġ�� �ʴ´�. |
| ���� ǥ�� | ���� ���: ��� �������������� �Է� ��ο� �б� ��� | �� �������� �ڷḦ �ٽ� �о� ǥ���Ѵ�. ����/�Խ�/�� ���� ������ ���� �ʴ´�. |

���� ���� ���� ���: `<config.materialRoot>/06_�ڵ� ���� ���`. ���� ���� ���: `<config.studioRoot>`, ȭ�� `<config.reviewServiceUrl>`. ��� ����� ���� �̵��� ������ �ʴ´�. ���� ��ġ�������͡����ȳ��� ���� �������� �ֽ� �ȳ��� �����Ѵ�. ��� ������ ���� ���� �Խ����� ���� �Ʒ�ó�� ��¥ ��ο� ���� ������ ����Ѵ�.

```json
{"materialRoot":"C:/EXAMPLE_ONLY/materials","studioRoot":"C:/EXAMPLE_ONLY/review","sourceRoot":"C:/EXAMPLE_ONLY/source","reviewServiceUrl":"http://127.0.0.1:9999","owners":{"production":"production-role","review":"review-role","buffer":"buffer-role"}}
```

�� ��ü�� ���� ������ ���� �����̸� ���� ���� ���� ��� ��Ű���� �������� �ʴ´�.

## ���� ������ �մ� ���� �ĺ���

| �ǹ� | ���� ���� | ���� ����/���� | approved handoff / Buffer ��� |
|---|---|---|---|
| �� ID | `entries[].id`, captions `postId` | `posts[].post_id`; ���� ���� `postId` | `postId` |
| ���� ���� | ���� `outputVersion` | `output_version` | `outputVersion` |
| ���� ���� ���� | captions `schema:"threads-publish-captions-v1"` | ���� ���� `schema:1`; ���´� ���� domain ��� | handoff `schema:1,type:"threads-final-review-handoff"`; provider ��� `schema:1` |
| ������ | status `title`; captions `originalTitle` | `source.original_title` | `source.original_title` |
| �Խÿ� ���� | plan `coverTitle`, captions `publicationTitle` | `source.cover_title/display_title`, `publication_title` | Threads `captions.threads`�� ���� ���� ǥ�� ���� |
| Instagram ���� | `platformCaptions.instagram` | `platform_captions.instagram`, legacy `caption` alias | `captions.instagram`�� �±׸� ���� ���� ���� |
| �±� | captions `topicTags`, `threadsTopicTag` | `common_tags/topic_tags/threads_topic_tag`; legacy `tags` | `tags.common/topic/legacy/threads_topic` |
| ����������Ʈ | status `images[].name/sha256` | `images[].asset_id/order/mime` | `images[].order/sha256/mime/local_file/approved_image_url` |
| ���� �������������޸� | ����/���� ���� �򰡿� ���� | ���� ���� `decision/outputVersion/fingerprint/reviewedAt/note/noteUpdatedAt`; ���� ���� ������ `final_review` | `review_statuses[].decision/current/reviewedRevision`; `approval.status/reviewedAt/fingerprint` |
| �÷����� ���ࡤ�Խ� | �ۼ����� �ʴ´� | ���� jobs�� �ܺ� ���� ���Ű� �ƴϴ� | results `platform/providerPostId/status/scheduledAt/publishedAt/externalUrl/providerVerifiedAt` |

���� ���� ����� ���� �ڵ�� ���� ������ `SHA256(JSON.stringify([sourceFingerprint, outputSha256, ruleVersion, images.map(i=>i.sha256), ...(reviewRound ? [reviewRound] : [])]))`�� ����Ѵ�. ���� ���� ���ڳ� ���� �����ð����� ��ü���� �ʴ´�. `reviewRound`�� truthy�� ���� ���Եȴ�. ���� identity�� �� ID���� ������sourceFingerprint�������񡤿���ó�� ���� status�� ��� ��ġ�ؾ� �Ѵ�.

### ���� ������ ���� ���� ? ���� ���� �߰� ���� ����

���� �԰��� �ǹ̿����� **content version**(���� ��� �ǹ̡����񡤹��ȡ����롤�̹��� ����/������ ����)�� **render version**(���� ������ �׸��� ���������� ���顤�ٹٲޡ����̾ƿ������� ����Ʈ�� ����)�� �����ؾ� �Ѵ�. �׷��� Ȯ���� ���� ��Ű������ �� ������ ���������� �޴� �ʵ尡 ����. ���� `outputVersion`�� `outputSha256`, ���� �̹��� �ؽ� �� ruleVersion ���� �����ϹǷ� ������ �޶����� �ٲ��. �� ����� �����ϸ� ���� `outputVersion`�� ���� �����̶�� ������ϰų� ��꿡�� �̹��� �ؽø� ���� �ʴ´�.

���� ��硤���� ��硤Buffer ����� ���� ���� �ʵ�� ������ ������ �ڿ��� ���� ����/���� ���� ������ �����Ѵ�. �ʿ��� ���� �ǹ̴� ���� ���� �ٰ�, ����/�� outputVersion, ������ ����/�� �̹��� �ؽ�, ���� ��Ģ, ���� ����, ������� ���� ���� ��ó, ���� ���� fingerprint, ���� �ڻ� ���� �����. ���⼭�� ������ �� � �ʵ���̳� ��Ű���� Ȯ������ �ʴ´�.

�� �÷����� ���� �� ID������ ���� ���������� ���� �̹��� �ؽ��� ���� ������ �ٷ��. Instagram ���Ȱ� Threads ǥ�� �����̶�� �÷����� ǥ�� ���̴� �����Ѵ�. �÷��� ���� ������ �Ϻ� �̹����� ���ų� ������ �ٲ� ������Ű�� �ʴ´�. �� �÷����� ���������� �� ����� �����ϰ� �ٸ� �÷����� ���/���з� ǥ���Ѵ�. �� �� `scheduled`/`sent`���� ������ ����Ѵ�. ���� domain�� Buffer adapter�� �÷����� ������ ���������� **�� ���� ������ �Խ�/�� �÷��� ���� ������ ������ ���� �ʴ�**. ���� ǥ��/��� ���� ��࿡�� �� ������ ���� Ȯ���ؾ� �Ѵ�.

## ���� �ڷ�� ���� ���� �ʵ�

PC �����񡤿������ٰš����� ���� �޸����� ��ϡ����� �̹����� ������ ���� ���� ��ġ�� �д�. `source`, `review`, `safety`, `blockers`, `local_file`, `consumer_contract.state_file`�� ��°�� �ܺ� API�� ���� GitHub�� ������ �ʴ´�. handoff���� �� ���� �ڷᰡ ���Ե� �� �����Ƿ� **handoff ��ü�� ���� ������ �ƴϴ�**.

Buffer�� ������ �ʵ�� ����ڰ� ������ ���� ���� `text`, ���ε� ���� �ڻ� URL�� ���� �迭, ���� ä��, ���� �ð�, �ʿ��� �÷��� metadata��. API�� ���� ���� ���ε带 ���� �ʴ´�. ���ε� �Խ� �̹����� ���� Cloudinary ���� ����� ���������� ������ HTTPS URL�� ����Ѵ�. Library �纹�糪 �� �����/����/���ѿ� �������� �ʴ´�. [Buffer ���� media ���](https://developers.buffer.com/guides/hosting-media.html).

GitHub���� ������ �ڵ塤�׽�Ʈ���� ��������¥ ���ø� �����Ѵ�. ����� ���� ���ο� ���� ��� �귣ġ�� ���� Ȯ���ؾ� �Ѵ�. �� ���� ���� �ʾ��� ���� GitHub�� �Խõ��� �ʾҴ�. ���� ������ JSON, ���� ���� ����, �̹���, ����, Ű/��ū�� ���� GitHub ������� ���� �ʴ´�.

## ����� ���� ����

���� ���� ������ `unreviewed/discard/revise/passed`��. ����� ���� output version, content basis, active ����, reviewed revision/time�� ���� ���� ��ȿ�ϴ�. ���ȡ��±ס�������/ǥ�� �����̹������������÷������������ð����Ǹ�/���� ���¸� �ٲٸ� ����� ���� dry-run/publication approval�� ��ȿȭ�ȴ�. ���� �򰡳� ���� immutable handoff�� ���� ����� ������� �ʴ´�.

handoff �Һ� ���� �ֽ� state revision, handoff ID, eligible �� ID, ���� fingerprint�� ����Ʈ SHA-256/MIME�� ��Ȯ���Ѵ�. `.local/final-review-decisions.json`�� �޸� ������ �������� �°����� �ʴ´�. ���� �����ȣ���� ���Ρ����� ����/�Խ� ������ ���� ������.

���� � Buffer ��Ͽ��� ���� ��ĸ� �����ϵ��� �� `explicit-title-edit-authorization-20261010.json`(schema 1, type `explicit-user-title-decoration-edit`)�� `titleEditBindings`�� �ִ�. ���� ��� outputVersion/fingerprint�� ���� ����/fingerprint, �÷���, providerPostId, authorizationFile, verifiedAt�� ����� �ִ�. �̰��� ���� ���¸� �ٽ� ����� ����� ����� �ƴϴ�. ���� ������ �ʵ塤��󡤺��游 ����ϸ� �̹��� ���� ������ ������ Ȯ������ �ʴ´�. generic domain���� ������ ���� ���� ������ ó���ϴ� �ʵ尡 �����Ƿ� **�ٸ� ���濡 �����Ϸ��� ������� ���� ���� ���� ��� Ȯ���� �ʿ��ϴ�**.

�ֽ� ����ڴ� **���̾ƿ� ������** ���� ���� ������ �°��ϵ��� ���� �����ߴ�. ���� ���� �԰ݡ����� ���顤�ٹٲ� ���� ���ε� ������ �ǹ̡����ȡ����롤�̹��� ����/������ �ٲ��� �ʾҴٴ� �ٰſ� ���� ������� ������ ���� ������ �����ؾ� �Ѵ�. �ǹ� ������ ��� ����� ����̴�. ���� �ڵ��� �⺻ ������ ���� �ؽ� ���浵 ������ ��ȿȭ�ϹǷ�, �� �°踦 �̹� �����Ѵٰ� �����ϰų� ���� ������ ���� ����� �ʴ´�. ����/����/Buffer ����� ȣȯ ��ࡤ���� ������ �ʿ��ϸ�, ���� ���� ��� ���� ������ ���̾ƿ� ���� �������� �������� �ʴ´�.

## ��Ρ����ϸ���MIME�� �Է� ����

���� outputFolder�� `06_�ڵ� ���� ���` �Ʒ����� �ؼ��ϰ�, �̹��� name�� outputFolder ��� `rendered/slide-001.png` ���´�. `\`�� �˻� �� `/`�� ����ȭ�Ѵ�. ���� ���/realpath�� �ش� root �Ʒ����� Ȯ���ϰ� Ż�⡤�ܺ� junction�� �����Ѵ�. ���� ���� �˻����� ������ ��θ� �������� �ʴ´�. �ѱ� ���ϸ��� UTF-8 JSON ���ڿ��� �����ϸ� OS path API�� ����Ѵ�.

���� �ڻ� ������ `.local/assets/<64�ڸ� sha256>`ó�� Ȯ���ڰ� ����. Ȯ���ڸ� �����̰ų� ���ϸ��� MIME �ٰŷ� ���� �ʴ´�. `.json` metadata�� MIME�� ���� JPEG/PNG/WebP magic bytes �� �ؽø� �Բ� Ȯ���Ѵ�. approved handoff�� local_file�� ������ ���� ����̸� approved_image_url�� null�̸� ���� ���� URL�� ���� ���̴�.

���� ����: ���� JSON ���� 20,000,000 bytes ����, entries 10,000 ����, �� ID 1~200��, �ߺ� ID ����, �۴� ���� image 200 ����, �̹��� ���� 25MiB ����. ���� selection�� 1~20��, �ߺ� ID �Ұ�. ���� PNG �̸���64�ڸ� �ҹ��� SHA-256������ ����Ʈ ��ġ�� �ʿ��ϴ�. captions�� �� �÷��� ���ڿ�, ���� publicationTitle, Instagram ù ���� `[ ���� ]`�� ���� 3~5�����̾�� �Ѵ�. topicTags�� ���ų� ���� �ٸ� 2��, Threads topic�� #/�ٹٲ� ���� 50�� ���ϴ�. ������ ���� ���� �ڵ��� �Է� �����̸� provider ���� ���ٴ� ���� �ƴϴ�.

`production_version_changed`, `production_image_changed`, `production_title_contract_changed`, `production_caption_identity_changed`, `handoff_asset_hash_mismatch`, `handoff_asset_mime_mismatch`�� �ش� �� ���� �ٰŴ�. ���� captions�� `awaiting-authored-captions`, �߸��� captions�� `held-...`; ������ ������ �������� �ʴ´�. stale CAS�� `revision_conflict`(409), ���� ���� ��import�� `same_version_import_conflict`, �ߺ� ���� `duplicate_post`��. ���� ���ϡ������� �ٽ� �а� ����ڰ� �ذ��Ѵ�.

## ���̾ƿ� ����°� �Ļ� �Խ� �̹��� ? ���ε� ����, ���� ��� ���� �ʿ�

����ڰ� ���� ������ �ڸ��� �ʴ� �ּ� ���� ���� �Ļ� �纻�� ���� �ּ� ���̸� �����ߴ�. ���� �ڵ� ����� ���� ����, ���� �ڻ� ������������ Buffer ����̴�. ���� ����Ʈ/���� ���/���� ���� ������ �����Ѵ�. ���ۿ� �Ļ� ������ ���� ��ο� �ؽø� ���´�.

���� ���� ǥ�������� �߸��� �߰ߵǾ� ���� ���α׷� ������ �غ� �̹��� ��ü�� ���� �԰ݡ����� ���顤�ٹٲ� ������� ��û�ƴ�. �ֽ� ���ô� ���� �Խá������� �����ϰ� �Խø� ���� �����ϸ鼭 �� ������ ������ �����ϴ� ���̴�. ������ �� render�� �������� ������ �ݿ��ϰ� �������������� �������� �ʴ´�. **���� ���� ��������� ũ�⡤���� ���� ��ġ�� ���� ����� ���� ����/�÷��� ���� ����� �޾� �����ؾ� �Ѵ�.** ������ ���� �̸�����, �ǵ� ����, ĳ���� ǥ��, Buffer ��θ� ���� ������ �������� �ʴ´�. �� ������ ���� ���� �԰��� ���ڸ� ���Ƿ� ������ �ʴ´�.

���� production/status �̹��� �� approved handoff �̹��� �ʵ忡�� �������Ļ� �ؽ�, padding��, �Ļ� ���� ���� �ʵ尡 ����. ���� �ʵ忡 �Ļ� �ؽø� ���� ���� �ؽ�ó�� ������ ���� �ٰŰ� �޶�����. **����� ������ ���� sidecar �Ǵ� ȣȯ ������ �߰� �ʵ塤�Һ��� ���� ������ �ʿ��ϴ�.** �ʿ��� ��� �ǹ̴� ���� ���/�ؽ�/ũ��, �Ļ� ���/�ؽ�/MIME/ũ��, top/bottom/left/right padding, no-crop ��ȯ ���, ���� ��ó/���/�ð�, ���� ��� version/fingerprint, �÷����� �����. ���⼭�� �� �ʵ� �̸�/��Ű���� � ������� ������ �ʴ´�.

���� 1080��552 ���� �м��� ����: Ȯ�� ��� Buffer ���� ������ Instagram �̹��� ������ `0.75 �� width/height �� 1.91`�� �����ϰ� ��� �ݿø��� ������� �ʾҴ�. �� 1.91 ��踸 ����ϸ� `ceil(1080/1.91)=566`�̸� 565px�� ���� ���̴�. **566px�� Ư�� ���� ������ ������ ���� ����� ��, �̹� ��ü ������� ���� ���� �԰ݡ����� ���� ������������ ���� ���߸� ������ �ƴϴ�.** ���� ���� ������ ���� ��� ���� ����� �ʿ��ϴ�. ���� ������������ ������ �� ������� Ȯ�ε��� �ʴ´�. [��� Ȯ���� Buffer ���� ���� ����](https://support.buffer.com/articles/instagrams-accepted-aspect-ratio-ranges-Frc2Xqewbd).

Instagram ĳ������ ù �� ������ ���� �� �̹����� crop�� �� �ִ�. �׷��� �� ���� �ּ� ���̸� ���ߴ� ������ ��ü ������ ���߸��� �������� ���Ѵ�. �� ���� ��� ���� ���� canvas ������ ���� padding�� �Բ� �����ؾ� �Ѵ�. Threads�� Instagram ��質 ĳ���� crop ��Ģ�� ���� �������� �ʴ´�. [Buffer �÷����� �̹��� ����](https://support.buffer.com/articles/ideal-image-sizes-and-formats-for-your-buffer-posts-JxHNGZFvf9).

Ȯ���� Buffer ������ Threads 4��, ���� local domain�� 20���� �����Ͽ� ���� ������ ����ġ�Ѵ�. �� ���̸� ���Ƿ� �����ϰų� �������� �ʴ´�. ���� ����ڰ� ����ϴ� Buffer connector/API�� ���� ���Ѱ� ������ Ȯ���ؾ� �Ѵ�. Meta ���� Instagram/Threads ������ �� ����ȯ�� web �������� ���� ������ �ʾ����Ƿ� �߰� ���ڸ� Ȯ������ �ʾҴ�. ���� readiness�� Instagram JPEG�� �䱸������ Buffer ������ PNG � �����ϹǷ� ��κ� ������ �ٸ��� �����Ѵ�.

## ������ �ݿ����ߺ������� �� �簳

���� StateStore�� ���� lock + CAS, ���� state backup, �ӽ� ���� exclusive ���� �� write �� fsync �� rename ������. ���� �� ���� ���/approval�� �ʱ�ȭ�ϰ� job�� stale�� �д�. approved handoff�� �ӽ� ���ϡ�fsync��hard-link�� immutable �� ���ϸ� �����Ѵ�. ���� semantic ID ������ ����ϰ� �ٸ� ������ �浹�� `handoff_collision`�� �����Ѵ�. producer captions�� temp+atomic rename�� ���� ������ ������ ����̸� �̹� ���� ����ڰ� ���� ������ ���� ������ �������� �ʾҴ�.

LocalAssets�� �ؽ� ���� exclusive ���⸦ �ϸ�, ����Ʈ+metadata �� ���� ��ü�� �ϳ��� transaction���� ���� ���� �ƴϴ�. �Һ� �� �� �� �����ϹǷ� �ҿ��� �ڻ��� ������Ѽ��� �� �ȴ�. �÷����� Buffer reservation�� durable �ߺ� fence�� ���� �����Ѵ�. provider idempotency�� �������� �ʴ´�. ���� ����/�κ� ���д� reconciliation���� �ΰ� providerPostId�� ������ �� ����ڰ� �簳�Ѵ�. �̹� ������ �÷����� �������ϰų� �� queue�� ��ü �������� �ؼ����� �ʴ´�.

## ���� Ȯ�� ������ ���� ����

- ���� ���� production/����/handoff/Buffer adapter �ڵ�� � ��� JSON�� **�ʵ�������������� ������** Ȯ���ߴ�. ���� ���ȡ����� �޸𡤿�������а��� ���ÿ� ������� �ʾҴ�.
- ���ô� ���� productionVersion/validateProductionCaptions ���� �Լ��� �����Ѵ�. ���� ���Ρ������ ���� ������ ���� ���� �������� �ʴ´�.
- �ǽð� �� ���б�, �ֽ� ��ġ �ݿ�, �� �÷��� �� gating, �Ļ� �ڻ� sidecar, ���� provider ���ѡ��ڻ� ���� ������ ���� ����/���� ����� �ʿ��ϴ�.
- content/render ���� ���� �ʵ� �� ���� ���ε� ���̾ƿ� ������ ���� �°� ������ ���� ��Ű�� ���������� �߰� ��ࡤ������ �ʿ��ϴ�. ���� ���� ��� �԰��� ���� ��� ���� ����.
- �ֽ� ��û�� ���� �Խù�������20�� ����, ��� �׽�Ʈ �� �÷��� ����5�� ������, ������ ������ ��ȸ �� ���� �̷� �ð� ����, ���� �ڵ�ȭ ��Ȱ��ȭ��. ���� Ȯ���� Buffer ��� ����� ������ �޾ƾ� �Ѵ�. ���� ���� ������ ���� ���� �ٰŷ� ������� �ʴ´�.
- ���� `BUFFER_CONTRACT.md`�� offline adapter �����̴�. � proof�� ���� Buffer ����� �ܺ� �۾� ����̹Ƿ� local API�� live��� �߷����� �ʴ´�.
- ������Ʈ ���� ��ü�� AGENTS/04/contracts�� ������. ���ȳ��� �����ϴ� �ҽ� `review-improvements-20261008-Sol/AGENTS.md`, `docs/HANDOFF_CONTRACTS.md`, `04_REVIEW_PUBLISH/README.md`�� Ȯ���ߴ�.

## �ڵ� �ٰ�

��� root `<config.studioRoot>` ����:

- `production-input.mjs:10` ����; `:13-16` root/���/JSON/�ߺ�; `:20-21` captions/title; `:24-32` �������ؽ� ��Ȯ��.
- `production-captions.mjs:5-17` schema, identity, ���ȡ��±� ����.
- `domain.mjs:6` ���� �÷��� ����; `:21-30` �̹���/MIME/������/Threads ����; `:39` basis; `:93-114` ������ ���� ��ȿȭ; `:132` ���� ����.
- `final-review.mjs:4-28` ������revision������ ���� ��ȿ��.
- `review-handoff.mjs:25-35` projection identity; `:90-104` �ؽá�MIME; `:120-132` ���� �ʵ塤�Һ��� �����; `:137-162` immutable ������ ����.
- `store.mjs:75-77` backup/���� ����/����; `local-assets.mjs:6-15` ������ ID�� exclusive �ڻ� ����.
- `buffer.mjs:30-49` �÷����� exact input/basis; `:72-104` �ߺ���reservation; `:111` ���� provider ������ reconciliation.

� read-only schema Ȯ��: `.local/final-review-decisions.json`, `.local/provider-delivery-log/verified-buffer-schedule-20261010.json`, ���� ������ `explicit-title-edit-authorization-20261010.json`. ���������� GitHub ��ũ�� �θ� �۾��� ����� ���� ���� �� ���� ��� �귣ġ���� �������Խ��� �� �����Ѵ�.
