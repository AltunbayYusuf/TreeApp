import { UserConfig, defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig(async () => {
    const config: UserConfig = {
        appType: 'custom',
        root: 'Assets',
        publicDir: 'public',
        plugins: [tailwindcss()],
        build: {
            emptyOutDir: true,
            manifest: true,
            outDir: '../wwwroot',
            assetsDir: '',
            rollupOptions: {
                input: 'Assets/main.ts'
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
                    api: "modern-compiler",
                },
            },
        },
    }

    return config;
});
