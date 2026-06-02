const fs = require('fs');
const data = fs.readFileSync('D:/teste/Sandswept/ror2-multi-language-pack/Sandswept/Translations/Zh-TW-Sandswept.language', 'utf8');
const lines = data.split('\n');
for (let i = 0; i < lines.length; i++) {
  const line = lines[i];
  if (line.includes('「') || line.includes('」')) {
    console.log('Line ' + (i+1) + ': ' + line.trim());
  }
}
