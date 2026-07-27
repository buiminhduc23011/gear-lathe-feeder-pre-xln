const fs = require('node:fs');
const path = require('node:path');

const modulePath = path.join(
  __dirname,
  '..',
  'node_modules',
  'react-native-tcp-socket',
  'android',
  'src',
  'main',
  'java',
  'com',
  'asterinet',
  'react',
  'tcpsocket',
  'TcpSocketModule.java',
);

const original = `    public void end(final Integer cId) {
        executorService.execute(new Runnable() {
            @Override
            public void run() {
                TcpSocketClient socketClient = getTcpClient(cId);
                socketClient.destroy();
            }
        });
    }`;

const patched = `    public void end(final Integer cId) {
        executorService.execute(new Runnable() {
            @Override
            public void run() {
                TcpSocket socket = socketMap.remove(cId);
                if (socket == null) {
                    Log.w(TAG, "Ignoring destroy for missing socket " + cId);
                    return;
                }
                if (!(socket instanceof TcpSocketClient)) {
                    Log.w(TAG, "Ignoring destroy for non-client socket " + cId);
                    return;
                }
                TcpSocketClient socketClient = (TcpSocketClient) socket;
                socketClient.destroy();
            }
        });
    }`;

if (!fs.existsSync(modulePath)) {
  console.warn(`[postinstall] react-native-tcp-socket source not found: ${modulePath}`);
  process.exit(0);
}

const source = fs.readFileSync(modulePath, 'utf8');
if (source.includes('Ignoring destroy for missing socket')) {
  console.log('[postinstall] react-native-tcp-socket destroy guard already applied');
  process.exit(0);
}

if (!source.includes(original)) {
  console.error('[postinstall] react-native-tcp-socket patch target changed; review TcpSocketModule.java');
  process.exit(1);
}

fs.writeFileSync(modulePath, source.replace(original, patched));
console.log('[postinstall] react-native-tcp-socket destroy guard applied');
