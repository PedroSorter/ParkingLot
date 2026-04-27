const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '/api'

export class ApiError extends Error {
  readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

export const requestJson = async <TResponse>(
  path: string,
  options: RequestInit = {},
): Promise<TResponse> => {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  })

  if (!response.ok) {
    const errorText = await response.text()
    throw new ApiError(getErrorMessage(errorText, response.statusText), response.status)
  }

  if (response.status === 204) {
    return undefined as TResponse
  }

  return (await response.json()) as TResponse
}

const getErrorMessage = (errorText: string, fallback: string) => {
  if (!errorText) {
    return fallback
  }

  try {
    const errorBody = JSON.parse(errorText) as { message?: unknown; title?: unknown }

    if (typeof errorBody.message === 'string') {
      return errorBody.message
    }

    if (typeof errorBody.title === 'string') {
      return errorBody.title
    }
  } catch {
    return errorText
  }

  return errorText
}
