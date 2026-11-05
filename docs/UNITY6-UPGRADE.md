# Unity 6 upgrade trial

This is an independent trial checkout. The original Unity 2022 project remains at D:/Project/SlotUnity.

- Baseline: 25f5b9fe1b51881f3290dc5b441b9a18500196e7, Unity 2022.3.24f1.
- Trial: Unity 6000.5.10f1, branch codex/unity6-upgrade.
- Assets/Scripts/Core, ReelColumn.cs and SlotGameController.cs are unchanged. The final candidate clears eight noninteractive Text raycast targets in the authored scene; the scene builder now creates noninteractive text too. This repairs pre-existing blocked plus buttons without changing gameplay rules.
- Unity upgraded the package lock, render pipeline resources and project settings.
- Added Editor UpgradeBuild tool and four PlayMode compatibility tests.

## Verification

EditMode: original 28 tests passed under Unity 6. PlayMode: four added tests passed, including 30 sequential reel cycles and repeated STOP requests. These Editor tests do not prove WebGL browser or mobile compatibility.

The isolated output, XML results, full logs, baseline hashes and review server are under:

D:/Codex/visualizations/2026-11-05/unity-slot-upgrade/

Build from a terminal using the installed Unity editor, -batchmode -nographics -quit, -projectPath D:/Project/SlotUnity-unity6, -buildTarget WebGL, -executeMethod Slot.EditorTools.UpgradeBuild.Build, and:

- -slotProfile development -slotOutput <new comparison output folder>
- -slotProfile release -slotOutput <new release output folder>

The tool only accepts new folders under the current D-drive comparison workspace; it never deletes an existing output. Development is uncompressed, release uses gzip; both retain Minimal stripping and full exception support. This isolates the effect of disabling development and transport compression without also changing stripping policy. Content hashes name generated resources. The original historical builder remains for comparison.

Start the local comparison server:

python D:/Codex/visualizations/2026-11-05/unity-slot-upgrade/review_server.py

Open http://127.0.0.1:8121/. It serves gzip with the matching Content-Encoding and Wasm MIME type. This is a local preview, not a production deployment.

## Rules intentionally left for a separate decision

1. Lua CorrectIine uses `value_2 ~= 9 or value_2 ~= 10`, which is always true. Unity intentionally excludes high-pay 9/10 from Wild substitution. Restoring raw Lua behavior and preserving the current corrected Unity behavior are different choices.
2. Both sources define paylines over only three of four indexed rows, but their vertical display/index mapping differs. Confirm the original visible/buffer rows before adding paylines or changing orientation.
3. Unity evaluates Branch * 6 lines while the original Result loops its full 30-line mapping.
4. Invalid/None symbols need validation; the upgrade comparison has not changed this pre-existing behavior.

Do not infer payout/RTP, a complete credit ledger, full-game fidelity or physical-phone support from these checks. Phone and standalone Chrome/Edge checks require separate actual devices/browsers.

## Completed comparison

- Unity 2022 original core files: 99,860,418 bytes.
- Unity 6 development comparison core files: 125,562,570 bytes.
- Unity 6 non-development gzip candidate with UI fix: 13,781,584 bytes.
- Approximately 86.2% fewer core-file bytes than the old uncompressed build, due to the combined build/compression configuration. The uncompressed Unity 6 build is larger; do not attribute the full reduction to the engine upgrade.
- The first development attempt failed with a Bee internal build-program rerun limit. An unchanged-settings retry succeeded; the failed report/log remain in the comparison folder. No claim of a confirmed root cause.
- Both successful pure upgrade builds and the final UI-fixed candidate were loaded in the in-app browser. SPIN/STOP and return to idle were observed. In the candidate, clicking plus centers changed 1 x 1 to 2 x 2 = 4; minus restored 1 x 1. Captured warn/error entries were empty.
- An additional batch UI raycast test produced no UI hits, both with and without a graphics device. It is not retained as a valid gate; its source and failed logs are kept with the artifacts. It must not be counted as a passing test. Real browser input verified the pointer fix.
- Successful development folder: unity6-development-v2. Final release folder: unity6-release-ui. The earlier unity6-release remains a pure-upgrade compressed comparison.
- Final candidate HTTP HEAD checks returned 200, gzip encoding, correct Wasm/JavaScript/data MIME types. No production deployment or physical phone validation has occurred.
