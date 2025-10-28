import PrimeUI from 'tailwindcss-primeui';

/** @type {import('tailwindcss').Config} */
export default {
  content: ['./src/**/*.{html,ts}'],
  darkMode: ['selector', '.mp-dark'],
  theme: {
    extend: {},
  },
  plugins: [PrimeUI]
}
