## 1.3.1

- Fix `ChargeUser` and `CancelSubscription` reporting failure when the API returns `"success": true` (success flags now accept `true`, `1`, `"1"`, `"true"`)
- Pass the backend reason (`error_msg`, then `message`) to callbacks, including for non-2xx responses such as 403 missing API key scope or 422 validation errors
- `FetchSubscriptions` / `HasActiveSubscription` now report the actual error instead of a fixed text

## 1.3.0

- Add `setApphudProfileID` and `setSuperwallProfileID` (sync web subscriptions to Apphud / Superwall)

## 1.2.0

- Send `device_model` header for identify fingerprinting (`SystemInfo.deviceModel`)

## 1.1.1

- Initial Unity version
- Add possibility to manage webview history back events

## 1.1.0

- Add webview for paywall and quizzes

## 1.0.0

- Initial version with subscription and user properties management
