const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');
const { join } = require('path');

module.exports = (_, { mode }) => {
  const isProduction = mode === 'production';
  const output = isProduction ? join(__dirname, 'build') : join(__dirname, '..', 'WebApp', 'wwwroot');

  return {
    mode,
    watch: !isProduction,
    devtool: isProduction ? 'cheap-module-source-map' : 'source-map',
    entry: {
      monaco: require.resolve('./src/monaco.js'),
    },
    output: {
      publicPath: '/',
      path: output,
      pathinfo: !isProduction,
      filename: 'js/[name].js',
      chunkFilename: `js/[name]${isProduction ? '.[hash:8]' : ''}.js`,
    },
    resolve: {
      extensions: ['.tsx', '.ts', '.js'],
    },
    module: {
      rules: [
        {
          test: /\.css$/,
          use: ['style-loader', 'css-loader'],
        },
      ],
    },
    plugins: [
      new MonacoWebpackPlugin({
        output: 'js',
        languages: ['typescript', 'javascript', 'json'],
      }),
    ],
  };
};
