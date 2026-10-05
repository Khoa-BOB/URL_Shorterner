export async function shortenUrl(value) {
  const longUrl = value.trim()
  let parsed
  try {
    parsed = new URL(longUrl)
  } catch {
    throw new Error('Enter a complete URL, starting with http:// or https://.')
  }
  if (!['http:', 'https:'].includes(parsed.protocol)) {
    throw new Error('Only HTTP and HTTPS URLs can be shortened.')
  }

  let response
  try {
    response = await fetch('/url/shorten', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ longUrl }),
      signal: AbortSignal.timeout(15000),
    })
  } catch {
    throw new Error('Could not reach the server. Check that the backend is running and try again.')
  }
  if (!response.ok) {
    throw new Error(response.status === 400
      ? 'The server rejected this URL. Check it and try again.'
      : 'Could not shorten your URL. Please try again.')
  }

  const result = await response.json()
  // Validate before rendering a clickable URL returned by the API.
  if (typeof result?.shortUrl !== 'string' || !URL.canParse(result.shortUrl) ||
      !['http:', 'https:'].includes(new URL(result.shortUrl).protocol)) {
    throw new Error('The server did not return a valid short URL. Please try again.')
  }
  return result.shortUrl
}
