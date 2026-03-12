/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./Views/**/*.{cshtml,html}",
        "./src/**/*.{js,ts,scss}",
    ],
    theme: {
        extend: {},
    },
    corePlugins: {
        visibility: false,
    },
    plugins: [],
}