/**
 * SOYYA WebGL プレビューサーバー
 * 
 * Unity WebGLビルド出力をローカルで正しくプレビューするためのサーバー
 * - gzip/Brotli圧縮ファイルの正しいContent-Type設定
 * - SharedArrayBuffer有効化のためのCOOP/COEPヘッダー
 * - SPA風フォールバック
 */

const http = require('http');
const fs = require('fs');
const path = require('path');

const PORT = process.env.PORT || 8080;
const BUILD_DIR = process.argv[2] || path.join(__dirname, '..', 'WebGL-Build');

// Unity WebGLビルド用MIMEタイプ
const MIME_TYPES = {
    '.html': 'text/html',
    '.js': 'application/javascript',
    '.wasm': 'application/wasm',
    '.data': 'application/octet-stream',
    '.json': 'application/json',
    '.css': 'text/css',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.svg': 'image/svg+xml',
    '.ico': 'image/x-icon',
    '.woff': 'font/woff',
    '.woff2': 'font/woff2',
    '.ttf': 'font/ttf',
    '.br': null,    // Brotli圧縮ファイル
    '.gz': null,    // gzip圧縮ファイル
    '.unityweb': null,  // Unity WebGL圧縮
};

// 圧縮ファイルのContent-Type解決
function resolveCompressedType(filePath) {
    const ext = path.extname(filePath);
    if (ext === '.br' || ext === '.gz' || ext === '.unityweb') {
        const baseName = filePath.replace(/\.(br|gz|unityweb)$/, '');
        const baseExt = path.extname(baseName);
        return {
            contentType: MIME_TYPES[baseExt] || 'application/octet-stream',
            encoding: ext === '.br' ? 'br' : 'gzip'
        };
    }
    return {
        contentType: MIME_TYPES[ext] || 'application/octet-stream',
        encoding: null
    };
}

const server = http.createServer((req, res) => {
    // COOP/COEP ヘッダー（SharedArrayBuffer有効化、Unityマルチスレッド対応）
    res.setHeader('Cross-Origin-Opener-Policy', 'same-origin');
    res.setHeader('Cross-Origin-Embedder-Policy', 'require-corp');
    res.setHeader('Cross-Origin-Resource-Policy', 'cross-origin');
    
    // CORS
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET');

    let requestPath = req.url.split('?')[0];
    if (requestPath === '/') requestPath = '/index.html';

    const filePath = path.join(BUILD_DIR, requestPath);

    // セキュリティ: ディレクトリトラバーサル防止
    if (!filePath.startsWith(BUILD_DIR)) {
        res.writeHead(403);
        res.end('Forbidden');
        return;
    }

    // ファイル検索（圧縮ファイル優先）
    let actualPath = filePath;
    let compressed = false;

    if (!fs.existsSync(filePath)) {
        // Brotli圧縮版を試行
        if (fs.existsSync(filePath + '.br')) {
            actualPath = filePath + '.br';
            compressed = true;
        } else if (fs.existsSync(filePath + '.gz')) {
            actualPath = filePath + '.gz';
            compressed = true;
        } else {
            res.writeHead(404);
            res.end('Not Found');
            return;
        }
    }

    const { contentType, encoding } = resolveCompressedType(actualPath);

    const headers = { 'Content-Type': contentType };
    if (encoding) {
        headers['Content-Encoding'] = encoding;
    }

    res.writeHead(200, headers);
    fs.createReadStream(actualPath).pipe(res);
});

// ===== 起動 =====
if (!fs.existsSync(BUILD_DIR)) {
    console.error(`\n❌ ビルドディレクトリが見つかりません: ${BUILD_DIR}`);
    console.error('先に WebGL ビルドを実行してください: .\\scripts\\build.ps1');
    process.exit(1);
}

server.listen(PORT, () => {
    console.log('\n============================================');
    console.log('  SOYYA WebGL プレビューサーバー');
    console.log('============================================');
    console.log(`  🌐 http://localhost:${PORT}`);
    console.log(`  📁 ${BUILD_DIR}`);
    console.log('  Ctrl+C で停止');
    console.log('============================================\n');
});
