# Web2Wave Unity

Web2Wave is a lightweight Unity package that provides a simple interface for managing user subscriptions and properties through a REST API.

## Features

- Fetch subscription status for users
- Check for active subscriptions
- Manage user properties
- Identify web2wave user via device fingerprinting
- Set third-party profiles (Adapty, RevenueCat, Qonversion)
- WebView integration for quizzes and landing pages
- Thread-safe singleton design
- Callback-based API
- Built-in error handling

## Installation

### Unity Package Manager (Git URL)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button and select `Add package from git URL...`
3. Enter: `https://github.com/web2wave/web2wave_unity.git`
4. Click `Add`

### Unity Package Manager (local package)

1. Download or clone this repository
2. In Unity, open `Window > Package Manager`
3. Click the `+` button and select `Add package from disk...`
4. Navigate to the package folder and select `package.json`

## Requirements

- Unity 2020.3 or higher
- Newtonsoft.Json (automatically included via dependency)

## Setup

Before using Web2Wave, you need to configure the API key:

```csharp
using Web2Wave;

// Initialize Web2Wave
Web2Wave.Shared.Initialize("your-api-key");
```

## Usage

### Checking Subscription Status

```csharp
// Fetch subscriptions
Web2Wave.Shared.FetchSubscriptions(
    "userID",
    onSuccess: (subscriptions) =>
    {
        Debug.Log($"User has {subscriptions.Count} subscriptions");
        foreach (var sub in subscriptions)
        {
            Debug.Log($"Subscription status: {sub.status}");
        }
    },
    onError: (error) =>
    {
        Debug.LogError($"Failed to fetch subscriptions: {error}");
    }
);

// Check if user has an active subscription
Web2Wave.Shared.HasActiveSubscription(
    "userID",
    onSuccess: (isActive) =>
    {
        Debug.Log($"User has active subscription: {isActive}");
    },
    onError: (error) =>
    {
        Debug.LogError($"Failed to check subscription: {error}");
    }
);
```

### External Subscription Cancel/Refund

```csharp
// Cancel subscription in external Stripe/Paddle/PayPal
Web2Wave.Shared.CancelSubscription(
    "sub_1PzNJzCsRq5tBi2bbfNsAf86", // paySystemId
    "optional comment", // comment (can be null)
    onComplete: (response) =>
    {
        if (response.IsSuccess)
        {
            Debug.Log("Subscription canceled");
        }
        else
        {
            Debug.LogError($"Failed to cancel: {response.ErrorMessage}");
        }
    }
);

// Refund subscription with invoiceID
Web2Wave.Shared.RefundSubscription(
    "sub_1PzNJzCsRq5tBi2bbfNsAf86", // paySystemId
    "invoice_id", // invoiceId
    "optional comment", // comment (can be null)
    onComplete: (response) =>
    {
        if (response.IsSuccess)
        {
            Debug.Log("Subscription refunded");
        }
        else
        {
            Debug.LogError($"Failed to refund: {response.ErrorMessage}");
        }
    }
);
```

### Managing User Properties

```csharp
// Fetch user properties
Web2Wave.Shared.FetchUserProperties(
    "userID",
    onSuccess: (properties) =>
    {
        foreach (var prop in properties)
        {
            Debug.Log($"{prop.Key}: {prop.Value}");
        }
    },
    onError: (error) =>
    {
        Debug.LogError($"Failed to fetch properties: {error}");
    }
);

// Update a user property
Web2Wave.Shared.UpdateUserProperty(
    "userID",
    "preferredTheme",
    "dark",
    onComplete: (response) =>
    {
        if (response.IsSuccess)
        {
            Debug.Log("Property updated successfully");
        }
        else
        {
            Debug.LogError($"Failed to update: {response.ErrorMessage}");
        }
    }
);
```

### Identify web2wave user

The `Identify()` method identifies a user using device fingerprinting and returns identification metadata including the `user_id`. Use it when a deeplink is unavailable.

```csharp
Web2Wave.Shared.Identify(
    onSuccess: (identificationData) =>
    {
        if (identificationData.success == 1 && !string.IsNullOrEmpty(identificationData.user_id))
        {
            Debug.Log($"Identified user: {identificationData.user_id}");

            Web2Wave.Shared.SetAdaptyProfileID(
                identificationData.user_id,
                "adaptyProfileID",
                onComplete: (_) => { }
            );
        }
    },
    onError: (error) =>
    {
        Debug.LogError($"Failed to identify user: {error}");
    }
);
```

**Response format:**

```json
{
  "success": 1,
  "user_id": "identified_user_guid",
  "match_method": "match_method_used",
  "platform": "iOS"
}
```

### Managing Third-Party Profiles

```csharp
// Save Adapty profileID
Web2Wave.Shared.SetAdaptyProfileID(
    "userID",
    "adaptyProfileID",
    onComplete: (response) =>
    {
        if (response.IsSuccess)
        {
            Debug.Log("Adapty profileID saved");
        }
    }
);

// Save RevenueCat profileID
Web2Wave.Shared.SetRevenuecatProfileID(
    "userID",
    "revenueCatProfileID",
    onComplete: (response) =>
    {
        if (response.IsSuccess)
        {
            Debug.Log("RevenueCat profileID saved");
        }
    }
);

// Save Qonversion profileID
Web2Wave.Shared.SetQonversionProfileID(
    "userID",
    "qonversionProfileID",
    onComplete: (response) =>
    {
        if (response.IsSuccess)
        {
            Debug.Log("Qonversion profileID saved");
        }
    }
);
```

### Working with Quiz or Landing Web Page

First, create a listener class:

```csharp
using Web2Wave;
using System.Collections.Generic;

public class WebViewListener : IWeb2WaveWebListener
{
    public void OnEvent(string eventName, Dictionary<string, object> data = null)
    {
        Debug.Log($"Event received: {eventName}");
    }

    public void OnClose(Dictionary<string, object> data = null)
    {
        Debug.Log("WebView closed");
        Web2WaveWebView.Instance.CloseWebPage();
    }

    public void OnQuizFinished(Dictionary<string, object> data = null)
    {
        Debug.Log("Quiz finished");
        Web2WaveWebView.Instance.CloseWebPage();
    }
}
```

Then open the web page:

```csharp
// Create listener instance
var listener = new WebViewListener();

// Open web page
Web2WaveWebView.Instance.OpenWebPage(
    "https://your-url.com",
    listener: listener,
    allowBackNavigation: true,
    backgroundColor: new Color(0x13 / 255f, 0x3C / 255f, 0x75 / 255f) // Optional, defaults to white
);

// Close web page programmatically
Web2WaveWebView.Instance.CloseWebPage();
```

**Note:** WebView functionality requires platform-specific implementations:
- **WebGL**: Requires JavaScript plugin (see WebView implementation)
- **Android/iOS**: Requires native plugins
- **Editor**: Falls back to opening URL in system browser

## API Reference

### `Web2Wave.Shared`

The singleton instance of the Web2Wave client.

#### Properties

- `bool IsInitialized` - Check if the SDK is initialized

#### Methods

##### `void Initialize(string apiKey)`

Initializes the Web2Wave SDK with your API key.

##### `void FetchSubscriptions(string web2waveUserId, Action<List<SubscriptionData>> onSuccess, Action<string> onError)`

Fetches the subscription status for a given user ID.

##### `void HasActiveSubscription(string web2waveUserId, Action<bool> onSuccess, Action<string> onError)`

Checks if the user has an active subscription (including trial status).

##### `void FetchUserProperties(string web2waveUserId, Action<Dictionary<string, string>> onSuccess, Action<string> onError)`

Retrieves all properties associated with a user.

##### `void UpdateUserProperty(string web2waveUserId, string property, string value, Action<Web2WaveResponse> onComplete)`

Updates a specific property for a user.

##### `void ChargeUser(string web2waveUserId, int priceId, Action<Web2WaveResponse> onComplete)`

Charge existing user with saved payment method.

##### `void CancelSubscription(string paySystemId, string comment, Action<Web2WaveResponse> onComplete)`

Cancel external subscription.

##### `void RefundSubscription(string paySystemId, string invoiceId, string comment, Action<Web2WaveResponse> onComplete)`

Refund external subscription.

##### `void SetRevenuecatProfileID(string web2waveUserId, string revenuecatProfileId, Action<Web2WaveResponse> onComplete)`

Set RevenueCat profileID.

##### `void SetAdaptyProfileID(string web2waveUserId, string adaptyProfileId, Action<Web2WaveResponse> onComplete)`

Set Adapty profileID.

##### `void SetQonversionProfileID(string web2waveUserId, string qonversionProfileId, Action<Web2WaveResponse> onComplete)`

Set Qonversion ProfileID.

##### `void Identify(Action<IdentifyResponse> onSuccess, Action<string> onError)`

Identifies a user using the device fingerprint and returns identification metadata.

### `Web2WaveWebView.Instance`

The singleton instance for managing webviews.

#### Methods

##### `void OpenWebPage(string webPageURL, IWeb2WaveWebListener listener = null, bool allowBackNavigation = false, Color? backgroundColor = null)`

Open web quiz or landing page.

##### `void CloseWebPage()`

Close web quiz or landing page.

## Platform Support

- ✅ **Unity Editor** - API calls work; WebView opens system browser
- ✅ **Android** - Full support (requires native WebView plugin)
- ✅ **iOS** - Full support (requires native WebView plugin)
- ✅ **WebGL** - API calls work; WebView requires JavaScript integration
- ⚠️ **Other platforms** - API calls work; WebView may have limited support

## License

MIT

## Support

- GitHub Issues: https://github.com/web2wave/web2wave_unity/issues
- Homepage: https://web2wave.com
