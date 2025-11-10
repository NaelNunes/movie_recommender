module.exports = {
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx,html}',
  ],
  theme: {
    extend: {
      colors: {
        'bg-dark': '#0b1020',
        'card-start': '#1f2440',
        'card-end': '#2b1b3a',
        'accent': '#7c5cff'
      },
      backgroundImage: {
        'grid-lines': "radial-gradient(circle at 10% 10%, rgba(124,92,255,0.06), transparent 1px), radial-gradient(circle at 90% 90%, rgba(124,92,255,0.04), transparent 1px)"
      }
    },
  },
  plugins: [],
}
