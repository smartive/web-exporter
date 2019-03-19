import 'monaco-editor';

monaco.languages.typescript.typescriptDefaults.setCompilerOptions({
  target: monaco.languages.typescript.ScriptTarget.ES2015,
  allowNonTsExtensions: true,
  checkJs: true,
  allowJs: true,
  strict: true,
});

monaco.languages.typescript.typescriptDefaults.addExtraLib(
  require('raw-loader!./editor-addons/test-mappings.d.ts'),
  './test-mappings.d.ts'
);
monaco.languages.typescript.typescriptDefaults.addExtraLib(
  require('raw-loader!@types/jsonpath/index.d.ts'),
  './jsonpath.d.ts'
);

const lodashContext = require.context('raw-loader!@types/lodash', true, /(index|common.*)[.]d[.]ts$/);

lodashContext.keys().forEach(key => {
  monaco.languages.typescript.typescriptDefaults.addExtraLib(lodashContext(key), key);
});
