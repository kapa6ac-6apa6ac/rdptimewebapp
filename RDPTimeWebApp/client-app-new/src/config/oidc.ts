export const oidcSettings =
{
  authority: "https://yapetlogon.snhpro.ru/auth/realms/master/",
  clientId: "ara",
  redirectUri: "http://ara.snhpro.ru/oidc-callback",
  popupRedirectUri: "http://ara.snhpro.ru/oidc-popup-callback",
  responseType: "id_token token",
  scope: "openid email",
  automaticSilentRenew: true,
  automaticSilentSignin: false,
  silentRedirectUri: "http://ara.snhpro.ru/silent-renew-oidc.html"
}