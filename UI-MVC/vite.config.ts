import { UserConfig, defineConfig } from 'vite';

export default defineConfig(async () => {
    const config: UserConfig = {
        appType: 'custom',
        base: '/dist/',
        root: 'src',
        publicDir: 'public',
        build: {
            emptyOutDir: true,
            manifest: true,
            outDir: '../wwwroot/dist',
            assetsDir: '',
            rollupOptions: {
                input: './src/main.ts',
                output: {
                    entryFileNames: 'main.js',
                    assetFileNames: 'main.css',
                }
            },
        },
        server: {
            strictPort: true,
        },
        optimizeDeps: {
            include: []
        },
        css: {
            preprocessorOptions: {
                scss: {
                },
            },
        },
    }

    return config;
});
