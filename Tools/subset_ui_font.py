"""Build the renamed OFL UI subset. Requires fonttools; source is the upstream Noto Sans TC variable TTF."""
from pathlib import Path
import argparse, json, hashlib
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont
from fontTools import subset

parser=argparse.ArgumentParser()
parser.add_argument('source',type=Path)
parser.add_argument('--report',type=Path)
args=parser.parse_args()
root=Path(__file__).resolve().parents[1]
font=instantiateVariableFont(TTFont(args.source),{'wght':500},inplace=True)
characters=set(chr(i) for i in range(32,127))
for source in (root/'Assets/Scripts').rglob('*.cs'):
    characters.update(c for c in source.read_text(encoding='utf-8-sig') if ord(c)>=160)
options=subset.Options();options.name_IDs=['*'];options.name_legacy=True
subsetter=subset.Subsetter(options=options)
subsetter.populate(unicodes=[ord(c) for c in characters]);subsetter.subset(font)
for record in font['name'].names:
    if record.nameID in (1,4,6,16):record.string='SlotSansTC'.encode(record.getEncoding())
    if record.nameID in (2,17):record.string='Medium'.encode(record.getEncoding())
output=root/'Assets/Resources/Fonts/SlotSansTC.ttf'
font.save(output)
# Every non-ASCII character in UI string literals must have a retained glyph.
ui_chars=set()
import re
for source in (root/'Assets/Scripts/Game').rglob('*.cs'):
    for literal in re.findall(r'"([^"\n]*)"',source.read_text(encoding='utf-8-sig')):
        ui_chars.update(c for c in literal if ord(c)>=160)
missing=sorted(ui_chars-set(chr(c) for c in font.getBestCmap()))
if missing:raise RuntimeError('Missing UI glyphs: '+''.join(missing))
report={'source':'https://github.com/google/fonts/tree/main/ofl/notosanstc',
        'sourceSha256':hashlib.sha256(args.source.read_bytes()).hexdigest(),
        'fontBytes':output.stat().st_size,'requestedCharacters':len(characters),
        'retainedCharacters':len(font.getBestCmap()),'missingUiGlyphs':missing}
if args.report:args.report.write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))
