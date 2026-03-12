import { UserConfig, defineConfig } from 'vite';

export default defineConfig(async () => {
    const config: UserConfig = {
        appType: 'custom',
        root: 'src',
        publicDir: 'public',
        build: {
            emptyOutDir: true,
            manifest: true,
            outDir: '../wwwroot/dist',
            assetsDir: '',
            rollupOptions: {
                input: './src/main.ts'
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
