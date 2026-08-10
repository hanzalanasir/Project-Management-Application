/**
 * Angular's HttpClient `params` option stringifies every object key via template literal
 * (`${value}`), so an `undefined` filter (e.g. an unset `search`/`status`) is sent to the API as
 * the literal query string `?search=undefined`, not omitted. The API then treats "undefined" as a
 * real filter value — for a string field that means a search for the literal word "undefined"
 * (matching nothing), and for a Guid field it's a 400 model-binding error. This strips undefined
 * and null entries before they ever reach HttpParams, so an unset filter is actually unset.
 */
export function toQueryParams<T extends object>(query: T): Record<string, string | number> {
  return Object.fromEntries(
    Object.entries(query as Record<string, unknown>).filter(([, value]) => value !== undefined && value !== null)
  ) as Record<string, string | number>;
}
