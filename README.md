# OneRing

OneRing is a dashboard for displaying reports collected from arbitrary data sources.

> Currently OneRing is based off of [this code sample,](https://github.com/microsoftgraph/aspnetcore-connect-sample) with the following instructions for running and debugging taken from that sample.

## Register the app

This app uses the Azure AD v2.0 endpoint, so you'll register it on the [App Registration Portal](https://apps.dev.microsoft.com/).

1. Sign into the portal using either your personal or work or school account.

2. Choose **Add an app** next to 'Converged applications'.

3. Enter a name for the app, and choose **Create application**. (Don't check the Guided Setup box.)

   a. Enter a friendly name for the application.

   b. Copy the **Application Id**. This is the unique identifier for your app.

   c. Under **Application Secrets**, choose **Generate New Password**. Copy the password from the dialog. You won't be able to access this value again after you leave this dialog.

   >**Important**: Note that in production apps you should always use certificates as your application secrets, but for this app we will use a simple shared secret password.

   d. Under **Platforms**, choose **Add platform**.

   e. Choose **Web**.

   f. Make sure the **Allow Implicit Flow** check box is selected, and add `https://localhost:44334/signin-oidc` as a **Redirect URL**. This is the base callback URL for this app.

   >The **Allow Implicit Flow** option enables the hybrid flow. During authentication, this enables the app to receive both sign-in info (the id_token) and artifacts (in this case, an authorization code) that the app can use to obtain an access token.

   g. Enter `https://localhost:44334/Account/SignOut` as the **Logout URL**.
  
   h. Click **Save**.

4. Configure Permissions for your application:  

   a. Choose **Microsoft Graph Permissions** > **Delegated Permissions** > **Add**.
  
   b. Select **openid**, **email**, **profile**, **offline_access**, **User.Read**, **User.ReadBasic.All** and **Mail.Send**. Then click **Ok**.
  
   c. Click **Save**.

You'll use the application ID and secret to configure the app in Visual Studio (or Visual Studio Code).
