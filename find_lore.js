const fs = require('fs');
const data = fs.readFileSync('D:/teste/Sandswept/ror2-multi-language-pack/Sandswept/Translations/En-Sandswept.language', 'utf8');
const idx = data.indexOf('CROWNS_DIAMOND_LORE');
if (idx >= 0) {
  const start = Math.max(0, idx - 10);
  const end = Math.min(data.length, idx + 200);
  console.log(data.substring(start, end));
}
