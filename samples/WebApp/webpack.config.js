var webpack = require('webpack');
var path = require('path');
module.exports = {
    entry: {
        app: "./Scripts/App.ts",
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
    externals: {
        "jquery": "jQuery",
    },
};
