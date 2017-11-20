var webpack = require('webpack');
var path = require('path');
module.exports = {
    entry: {
        app: "./Scripts/App.ts",
        vendor: [
            "core-js",
            "gluon-client"
        ]
    },
    output: {
        filename: "./dist/bundle.js",
    },
    devtool: 'source-map',
    resolve: {
        extensions: [".webpack.js", ".web.js", ".ts", ".js"],
    },
    module: {
        rules: [{
                test: /\.tsx?$/,
                exclude: /node_modules/,
                loader: "ts-loader"
            },
            {
                enforce: "pre",
                test: /\.js$/,
                loader: "source-map-loader"
            },
        ]
    },
    plugins: [
        new webpack.optimize.CommonsChunkPlugin({ name: "vendor", filename: "./dist/vendor.bundle.js" })
    ]
};
