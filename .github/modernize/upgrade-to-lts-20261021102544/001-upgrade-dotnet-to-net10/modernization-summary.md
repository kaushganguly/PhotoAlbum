finalStatus: failed

successCriteriaStatus:
  passBuild: true
  generateNewUnitTests: false
  passUnitTests: true
  securityComplianceCheck: false

summary: >
  Retargeted PhotoAlbum and PhotoAlbum.Tests to net10.0 and upgraded EF Core,
  ASP.NET Core testing, xUnit, test SDK, coverlet, and ImageSharp to compatible
  versions. Updated System.Security.Cryptography.Pkcs to 10.0.13 to resolve
  vulnerable transitive use, and validate detected image content against its
  declared MIME type before decoding. Release build succeeded and all 7 existing
  tests passed; no tests were added.

failureReason: >
  NuGet's vulnerability audit still reports multiple advisories for
  SixLabors.ImageSharp 3.1.12. The first patched version is 4.1.2, but the
  available 4.1.3 package requires a valid SixLabors license for Release builds,
  and no license key is configured or available. The security check therefore
  cannot be marked compliant without a license decision or replacing the image
  processing dependency.
