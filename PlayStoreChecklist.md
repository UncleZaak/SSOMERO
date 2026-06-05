Play Store Readiness Checklist for Ssomero Android

1. Build a signed AAB using a release keystore (do NOT check the keystore into source control).
2. Verify versionCode and versionName are correct and incremented.
3. Ensure AndroidManifest has no debug=true flags and networkSecurityConfig does not permit cleartext in Release.
4. Remove any localhost or 10.0.2.2 references from configuration in Release builds.
5. Verify all permissions are justified and listed in the privacy policy.
6. Confirm SecureStorage behavior across API levels and migration of Preferences.
7. Run lint and permission checks, test on Android 8, 11, 13, 14 where possible.
8. Validate in-app billing (if used) and payment endpoints over HTTPS.
9. Prepare store listing assets (icons, screenshots, privacy policy URL).
10. Complete internal testing tracks before public release.
