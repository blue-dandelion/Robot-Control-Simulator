// metro.config.js
const { getDefaultConfig } = require('expo/metro-config');

module.exports = (() => {
    const config = getDefaultConfig(__dirname);

    // Remove "svg" from assetExts, add it to sourceExts
    config.resolver.assetExts = config.resolver.assetExts.filter(ext => ext !== 'svg');
    config.resolver.sourceExts = [...config.resolver.sourceExts, 'svg'];

    // Tell Metro to use the SVG transformer
    config.transformer.babelTransformerPath = require.resolve('react-native-svg-transformer');

    return config;
})();