const OptimizeCssAssetsPlugin = require('optimize-css-assets-webpack-plugin');
const { CheckerPlugin } = require('awesome-typescript-loader');
const autoprefixer = require('autoprefixer');
const CleanWebpackPlugin = require('clean-webpack-plugin');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const { join } = require('path');

module.exports = (_, { mode }) => {
  const isProduction = mode === 'production';
  const output = isProduction ? join(__dirname, 'build') : join(__dirname, '..', 'WebApp', 'wwwroot');

  return {
    mode,
    watch: !isProduction,
    devtool: isProduction ? 'cheap-module-source-map' : 'source-map',
    entry: {
      app: require.resolve('./src/index.ts'),
    },
    output: {
      publicPath: '/',
      path: output,
      pathinfo: !isProduction,
      filename: 'js/[name].js',
    },
    resolve: {
      extensions: ['.tsx', '.ts', '.js'],
    },
    module: {
      rules: [
        {
          oneOf: [
            {
              test: /\.tsx?$/,
              loader: 'awesome-typescript-loader',
            },
            {
              test: /\.scss$/,
              loader: ExtractTextPlugin.extract(
                Object.assign({
                  fallback: require.resolve('style-loader'),
                  use: [
                    {
                      loader: require.resolve('css-loader'),
                      options: {
                        importLoaders: 1,
                        sourceMap: true,
                      },
                    },
                    {
                      loader: require.resolve('postcss-loader'),
                      options: {
                        ident: 'postcss',
                        sourceMap: true,
                        plugins: () => [
                          require('postcss-flexbugs-fixes'),
                          autoprefixer({
                            browsers: ['>1%', 'last 4 versions', 'Firefox ESR', 'not ie < 11'],
                            flexbox: 'no-2009',
                          }),
                        ],
                      },
                    },
                    'resolve-url-loader',
                    { loader: 'sass-loader', options: { sourceMap: true } },
                  ],
                })
              ),
            },
            {
              test: /\.css$/,
              loader: ExtractTextPlugin.extract(
                Object.assign({
                  fallback: require.resolve('style-loader'),
                  use: [
                    {
                      loader: require.resolve('css-loader'),
                      options: {
                        importLoaders: 1,
                        sourceMap: true,
                      },
                    },
                    {
                      loader: require.resolve('postcss-loader'),
                      options: {
                        ident: 'postcss',
                        plugins: () => [
                          require('postcss-flexbugs-fixes'),
                          autoprefixer({
                            browsers: ['>1%', 'last 4 versions', 'Firefox ESR', 'not ie < 11'],
                            flexbox: 'no-2009',
                          }),
                        ],
                      },
                    },
                  ],
                })
              ),
            },
            {
              test: /favicon/,
              loader: require.resolve('file-loader'),
              options: {
                name: 'media/[name].[ext]',
              },
            },
            {
              loader: require.resolve('file-loader'),
              exclude: [/\.(j|t)sx?$/, /\.html$/, /\.json$/],
              options: {
                name: 'media/[name].[hash:8].[ext]',
              },
            },
          ],
        },
      ],
    },
    plugins: [
      new CleanWebpackPlugin(output, {
        allowExternal: true,
      }),
      new ExtractTextPlugin({
        filename: 'css/[name].css',
      }),
      new CheckerPlugin(),
      isProduction ? new OptimizeCssAssetsPlugin() : null,
    ].filter(Boolean),
  };
};
