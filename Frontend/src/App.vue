<script setup>
import { ref } from 'vue'
import { shortenUrl } from './api.js'

const longUrl = ref('')
const shortUrl = ref('')
const loading = ref(false)
const error = ref('')
const copyStatus = ref('')

async function shorten() {
  if (loading.value) return
  loading.value = true
  shortUrl.value = ''
  error.value = ''
  copyStatus.value = ''
  try {
    shortUrl.value = await shortenUrl(longUrl.value)
  } catch (cause) {
    error.value = cause instanceof SyntaxError
      ? 'The server returned an unexpected response. Please try again.'
      : cause.message
  } finally {
    loading.value = false
  }
}

async function copy() {
  try {
    await navigator.clipboard.writeText(shortUrl.value)
    copyStatus.value = 'Copied to clipboard!'
  } catch {
    copyStatus.value = 'Copy unavailable. Select the link and copy it manually.'
  }
}
</script>

<template>
  <main>
    <header class="brand"><span class="brand-mark" aria-hidden="true">↗</span> URL Shortener</header>
    <section class="card" aria-labelledby="title">
      <p class="eyebrow">LESS LINK. MORE POSSIBILITY.</p>
      <h1 id="title">Long link?<br /><span>Keep it short.</span></h1>
      <p class="intro">Turn a long URL into a simple link you can share anywhere.</p>

      <form @submit.prevent="shorten" :aria-busy="loading">
        <label for="long-url">Paste your long URL</label>
        <div class="input-row">
          <input id="long-url" v-model="longUrl" type="url" required
            placeholder="https://example.com/your-long-link" autocomplete="url"
            :disabled="loading" :aria-invalid="error ? 'true' : undefined"
            :aria-describedby="error ? 'url-hint url-error' : 'url-hint'" />
          <button type="submit" :disabled="loading">
            {{ loading ? 'Shortening…' : 'Shorten URL' }}<span v-if="!loading" aria-hidden="true"> ↗</span>
          </button>
        </div>
        <p id="url-hint" class="hint">Use a complete link starting with http:// or https://.</p>
        <p v-if="error" id="url-error" class="error" role="alert">{{ error }}</p>
      </form>

      <div aria-live="polite" aria-atomic="true">
        <div v-if="shortUrl" class="result">
          <p class="result-label">Your short link is ready</p>
          <div class="result-row">
            <a :href="shortUrl" target="_blank" rel="noopener noreferrer">{{ shortUrl }}</a>
            <button type="button" class="copy-button" @click="copy">Copy link</button>
          </div>
          <p v-if="copyStatus" class="hint">{{ copyStatus }}</p>
        </div>
      </div>
    </section>
    <footer>Shorten. Copy. Share.</footer>
  </main>
</template>

<style>
:root { font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; color: #203c35; background: #f4f7f3; font-synthesis: none; }
* { box-sizing: border-box; }
body { margin: 0; }
main { max-width: 850px; margin: 0 auto; padding: 48px 24px; }
.brand { display: flex; align-items: center; gap: 10px; font-weight: 700; font-size: 18px; }
.brand-mark { display: grid; place-items: center; background: #176b57; color: white; width: 32px; height: 32px; border-radius: 9px; font-size: 23px; }
.card { margin-top: 72px; padding: 48px; border: 1px solid #dde6de; border-radius: 24px; background: white; box-shadow: 0 12px 48px #203c3506; }
.eyebrow { margin: 0 0 18px; color: #647b6f; font-size: 11px; font-weight: 700; letter-spacing: 2px; }
h1 { margin: 0; font-size: clamp(38px, 7vw, 60px); line-height: 1.08; letter-spacing: -2px; }
h1 span { color: #176b57; }
.intro { margin: 22px 0 36px; color: #63716c; line-height: 1.7; }
label { display: block; margin-bottom: 10px; font-size: 14px; font-weight: 600; }
.input-row, .result-row { display: flex; gap: 10px; align-items: center; }
input { width: 100%; min-width: 0; padding: 15px; border: 1px solid #c9d6ce; border-radius: 10px; font: inherit; font-size: 14px; background: #fcfdfb; color: #203c35; }
button { flex-shrink: 0; padding: 15px 20px; border: 1px solid #176b57; border-radius: 10px; background: #176b57; color: white; font: inherit; font-size: 14px; font-weight: 600; cursor: pointer; }
button:hover { background: #105340; }
button:disabled { opacity: .65; cursor: wait; }
:focus-visible { outline: 3px solid #84bca6; outline-offset: 3px; }
.hint { margin: 12px 0 0; color: #65766e; font-size: 12px; line-height: 1.6; }
.error { color: #a42c32; font-size: 14px; line-height: 1.6; }
.result { margin-top: 28px; padding: 20px; background: #edf6ef; border: 1px solid #d0e7d8; border-radius: 12px; }
.result-label { margin: 0 0 12px; color: #4c6c5c; font-size: 12px; font-weight: 600; }
.result a { flex: 1; min-width: 0; overflow-wrap: anywhere; color: #176b57; font-weight: 600; line-height: 1.6; }
.copy-button { background: white; color: #176b57; padding: 10px 14px; }
.copy-button:hover { background: #e0ede4; }
footer { margin-top: 28px; text-align: center; color: #708077; font-size: 12px; }
@media (max-width: 600px) {
  main { padding: 28px 16px; }
  .card { margin-top: 40px; padding: 28px 22px; }
  .input-row, .result-row { flex-direction: column; align-items: stretch; }
  .result-row { gap: 16px; }
}
</style>
