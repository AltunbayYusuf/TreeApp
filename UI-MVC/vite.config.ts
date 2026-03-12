import { UserConfig, defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig(async () => {
    const config: UserConfig = {
        appType: 'custom',
        root: 'src',
        publicDir: 'public',
        plugins: [tailwindcss()],
        build: {
            emptyOutDir: true,
            manifest: true,
            outDir: '../wwwroot/dist',
            assetsDir: '',
            rollupOptions: {
                input: 'main.ts'
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
