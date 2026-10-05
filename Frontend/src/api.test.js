import assert from 'node:assert/strict'
import { test } from 'node:test'
import { shortenUrl } from './api.js'

test('shortening validates URLs, sends the backend contract, and handles failures', async (t) => {
  const mock = t.mock.method(globalThis, 'fetch', async () =>
    Response.json({ shortUrl: 'http://localhost:5001/abc123' }))

  for (const value of ['', 'not a URL', 'ftp://example.com']) {
    await assert.rejects(shortenUrl(value), /URL/)
  }
  assert.equal(mock.mock.callCount(), 0)
  assert.equal(await shortenUrl('  https://example.com/path?q=hello&x=1  '), 'http://localhost:5001/abc123')
  const [endpoint, options] = mock.mock.calls[0].arguments
  assert.equal(endpoint, '/url/shorten')
  assert.equal(options.method, 'POST')
  assert.deepEqual(JSON.parse(options.body), { longUrl: 'https://example.com/path?q=hello&x=1' })

  mock.mock.mockImplementation(async () => new Response('', { status: 400 }))
  await assert.rejects(shortenUrl('https://example.com'), /rejected/)
  mock.mock.mockImplementation(async () => new Response('', { status: 500 }))
  await assert.rejects(shortenUrl('https://example.com'), /Could not shorten/)
  mock.mock.mockImplementation(async () => { throw new TypeError('Failed to fetch') })
  await assert.rejects(shortenUrl('https://example.com'), /Could not reach/)
  mock.mock.mockImplementation(async () => Response.json({ shortUrl: 'javascript:alert(1)' }))
  await assert.rejects(shortenUrl('https://example.com'), /valid short URL/)
})
