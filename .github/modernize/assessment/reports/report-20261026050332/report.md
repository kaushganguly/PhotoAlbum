# PhotoAlbum

> **Assessment limitations:** AppCAT reported “Msbuild was not found” for both projects on Linux before completing analysis; cloud-readiness findings may be incomplete. Security analysis was static, without live exploitation or deployed-resource verification. Some CWE classifications overlap, and dependency findings do not establish API reachability. No application vulnerabilities were remediated. See [security findings and evidence](security/security-assessment.md).

## Summary

| Metric | Value |
|--------|-------|
| Total Issues | 24 |
| Mandatory Blockers | 7 |
| Potential Issues | 7 |

## Component Information

| Property | Value |
|----------|-------|
| Language | C# |
| Frameworks | net9.0 |
| Build tools | MSBuild |

## Cloud Readiness Issues

| Issue Name | Criticality | Story Points | Occurrences |
|------------|-------------|--------------|-------------|
| Windows authentication detected | Mandatory | 3 | [1](#Windows_authentication_detected) |
| Local or network IO operations detected | Potential | 3 | [18](#Local_or_network_IO_operations_detected) |
| Local application configuration detected | Potential | 1 | [2](#Local_application_configuration_detected) |
| Connection string is detected | Potential | 3 | [1](#Connection_string_is_detected) |
| SQL database connection detected | Potential | 3 | [1](#SQL_database_connection_detected) |
| Static content detected | Optional | 3 | [1](#Static_content_detected) |
| Connection strings without configuration builders detected | Optional | 3 | [1](#Connection_strings_without_configuration_builders_detected) |

### Issue Details

<details id="Windows_authentication_detected">
<summary><b>Windows authentication detected</b> — affected files</summary>

- `PhotoAlbum/Web.config`

</details>

<details id="Local_or_network_IO_operations_detected">
<summary><b>Local or network IO operations detected</b> — affected files</summary>

- `PhotoAlbum/Program.cs (line 30)`
- `PhotoAlbum/Program.cs (line 28)`
- `PhotoAlbum/Services/PhotoService.cs (line 116)`
- `PhotoAlbum/Services/PhotoService.cs (line 185)`
- `PhotoAlbum/Services/PhotoService.cs (line 228)`
- `PhotoAlbum/Services/PhotoService.cs (line 114)`
- `PhotoAlbum/Services/PhotoService.cs (line 183)`
- `PhotoAlbum/Services/PhotoService.cs (line 226)`
- `PhotoAlbum/Services/PhotoService.cs (line 142)`
- `PhotoAlbum/Pages/PhotoFile.cshtml.cs (line 60)`
- `PhotoAlbum/Pages/PhotoFile.cshtml.cs (line 66)`
- `PhotoAlbum/Pages/PhotoFile.cshtml.cs (line 58)`
- `PhotoAlbum.Tests/Unit/Services/PhotoServiceTests.cs (line 28)`
- `PhotoAlbum.Tests/Unit/Services/PhotoServiceTests.cs (line 207)`
- `PhotoAlbum.Tests/Unit/Services/PhotoServiceTests.cs (line 121)`
- `PhotoAlbum.Tests/Unit/Services/PhotoServiceTests.cs (line 29)`
- `PhotoAlbum.Tests/Unit/Services/PhotoServiceTests.cs (line 226)`
- `PhotoAlbum.Tests/Unit/Services/PhotoServiceTests.cs (line 224)`

</details>

<details id="Local_application_configuration_detected">
<summary><b>Local application configuration detected</b> — affected files</summary>

- `PhotoAlbum/appsettings.json`
- `PhotoAlbum/appsettings.json`

</details>

<details id="Connection_string_is_detected">
<summary><b>Connection string is detected</b> — affected files</summary>

- `PhotoAlbum/appsettings.json`

</details>

<details id="SQL_database_connection_detected">
<summary><b>SQL database connection detected</b> — affected files</summary>

- `PhotoAlbum/Web.config`

</details>

<details id="Static_content_detected">
<summary><b>Static content detected</b> — affected files</summary>

- `PhotoAlbum/PhotoAlbum.csproj`

</details>

<details id="Connection_strings_without_configuration_builders_detected">
<summary><b>Connection strings without configuration builders detected</b> — affected files</summary>

- `PhotoAlbum/Web.config`

</details>

## DotNET Upgrade Issues [View Details](scenarios/dotnet-version-upgrade/assessment.md)

| Issue Category | Criticality | Story Points | Occurrences |
|----------------|-------------|--------------|-------------|
| Binary incompatible for selected .NET version | Mandatory | 1 | [3](#Binary_incompatible_for_selected_NET_version) |
| Project's target framework(s) needs to be changed | Mandatory | 1 | [2](#Project_s_target_framework_s_needs_to_be_changed) |
| NuGet package upgrade is recommended | Potential | 1 | [4](#NuGet_package_upgrade_is_recommended) |
| Behavioral change in selected .NET version | Potential | 1 | [1](#Behavioral_change_in_selected_NET_version) |
| NuGet package contains security vulnerability | Optional | 1 | [1](#NuGet_package_contains_security_vulnerability) |
| NuGet package is deprecated | Optional | 1 | [1](#NuGet_package_is_deprecated) |

### Issue Details

<details id="Binary_incompatible_for_selected_NET_version">
<summary><b>Binary incompatible for selected .NET version</b> — affected files</summary>

- `PhotoAlbum/Services/PhotoService.cs (line 30, col 8)`
- `PhotoAlbum/Program.cs (line 34, col 0)`

</details>

<details id="Project_s_target_framework_s_needs_to_be_changed">
<summary><b>Project's target framework(s) needs to be changed</b> — affected files</summary>

- `PhotoAlbum/PhotoAlbum.csproj`
- `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj`

</details>

<details id="NuGet_package_upgrade_is_recommended">
<summary><b>NuGet package upgrade is recommended</b> — affected files</summary>

- `PhotoAlbum/PhotoAlbum.csproj`
- `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj`

</details>

<details id="Behavioral_change_in_selected_NET_version">
<summary><b>Behavioral change in selected .NET version</b> — affected files</summary>

- `PhotoAlbum/Program.cs (line 54, col 4)`

</details>

<details id="NuGet_package_contains_security_vulnerability">
<summary><b>NuGet package contains security vulnerability</b> — affected files</summary>

- `PhotoAlbum/PhotoAlbum.csproj`

</details>

<details id="NuGet_package_is_deprecated">
<summary><b>NuGet package is deprecated</b> — affected files</summary>

- `PhotoAlbum.Tests/PhotoAlbum.Tests.csproj`

</details>

## Security Issues

> **Note:** These issues were generated by AI and may contain inaccuracies or incomplete information. Please review carefully.

| Issue Name | Criticality | Story Points | Files |
|------------|-------------|--------------|-------|
| CWE-434: Unrestricted Upload of File with Dangerous Type | Mandatory | 8 | [4](#CWE-434_Unrestricted_Upload_of_File_with_Dangerous_Type) |
| CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer | Mandatory | 1 | [1](#CVE-2026-106115_ImageSharp_TIFF_CCITT_T6_encoder_writes_beyond_an_undersized_output_buffer) |
| CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer | Mandatory | 1 | [1](#CVE-2026-106110_ImageSharp_TIFF_CCITT_T4_encoder_can_write_past_its_compressed_output_buffer) |
| CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index | Mandatory | 1 | [1](#CVE-2026-106113_ImageSharp_HistogramEqualization_uses_an_unvalidated_luminance_as_an_unchecked_histogram_index) |
| CWE-789: Memory Allocation with Excessive Size Value | Potential | 5 | [3](#CWE-789_Memory_Allocation_with_Excessive_Size_Value) |
| CWE-79: Improper Neutralization of Input During Web Page Generation ('Cross-site Scripting') | Optional | 8 | [4](#CWE-79_Improper_Neutralization_of_Input_During_Web_Page_Generation_Cross-site_Scripting) |
| CWE-259: Use of Hard-coded Password | Optional | 5 | [2](#CWE-259_Use_of_Hard-coded_Password) |
| CWE-732: Incorrect Permission Assignment for Critical Resource | Optional | 5 | [4](#CWE-732_Incorrect_Permission_Assignment_for_Critical_Resource) |
| CWE-798: Use of Hard-coded Credentials | Optional | 5 | [3](#CWE-798_Use_of_Hard-coded_Credentials) |
| CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop | Optional | 1 | [1](#CVE-2026-106116_ImageSharp_BigTIFF_IFD_count_can_keep_a_decoder_thread_in_a_non-progressing_loop) |
| CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions | Optional | 1 | [1](#CVE-2026-106114_ImageSharp_ICC_CLUT_parsing_allocates_from_unvalidated_channel_and_grid_dimensions) |

### Security Issue Details

<details id="CWE-434_Unrestricted_Upload_of_File_with_Dangerous_Type">
<summary><b>CWE-434: Unrestricted Upload of File with Dangerous Type</b> — affected files</summary>

- `PhotoAlbum/Services/PhotoService.cs:82`
- `PhotoAlbum/Services/PhotoService.cs:110`
- `PhotoAlbum/Services/PhotoService.cs:134`
- `PhotoAlbum/Program.cs:63`

</details>

<details id="CVE-2026-106115_ImageSharp_TIFF_CCITT_T6_encoder_writes_beyond_an_undersized_output_buffer">
<summary><b>CVE-2026-106115: ImageSharp: TIFF CCITT T6 encoder writes beyond an undersized output buffer</b> — affected files</summary>

- `PhotoAlbum/PhotoAlbum.csproj:16`

</details>

<details id="CVE-2026-106110_ImageSharp_TIFF_CCITT_T4_encoder_can_write_past_its_compressed_output_buffer">
<summary><b>CVE-2026-106110: ImageSharp: TIFF CCITT T4 encoder can write past its compressed output buffer</b> — affected files</summary>

- `PhotoAlbum/PhotoAlbum.csproj:16`

</details>

<details id="CVE-2026-106113_ImageSharp_HistogramEqualization_uses_an_unvalidated_luminance_as_an_unchecked_histogram_index">
<summary><b>CVE-2026-106113: ImageSharp: HistogramEqualization uses an unvalidated luminance as an unchecked histogram index</b> — affected files</summary>

- `PhotoAlbum/PhotoAlbum.csproj:16`

</details>

<details id="CWE-789_Memory_Allocation_with_Excessive_Size_Value">
<summary><b>CWE-789: Memory Allocation with Excessive Size Value</b> — affected files</summary>

- `PhotoAlbum/Services/PhotoService.cs:92`
- `PhotoAlbum/Services/PhotoService.cs:127`
- `PhotoAlbum/Pages/Index.cshtml.cs:53`

</details>

<details id="CWE-79_Improper_Neutralization_of_Input_During_Web_Page_Generation_Cross-site_Scripting">
<summary><b>CWE-79: Improper Neutralization of Input During Web Page Generation ('Cross-site Scripting')</b> — affected files</summary>

- `PhotoAlbum/wwwroot/js/upload.js:166`
- `PhotoAlbum/wwwroot/js/upload.js:193`
- `PhotoAlbum/wwwroot/js/upload.js:224`
- `PhotoAlbum/Pages/Index.cshtml.cs:77`

</details>

<details id="CWE-259_Use_of_Hard-coded_Password">
<summary><b>CWE-259: Use of Hard-coded Password</b> — affected files</summary>

- `infra/main.bicep:59`
- `infra/main.bicep:68`

</details>

<details id="CWE-732_Incorrect_Permission_Assignment_for_Critical_Resource">
<summary><b>CWE-732: Incorrect Permission Assignment for Critical Resource</b> — affected files</summary>

- `PhotoAlbum/Pages/Detail.cshtml.cs:92`
- `PhotoAlbum/Services/PhotoService.cs:212`
- `PhotoAlbum/Program.cs:8`
- `PhotoAlbum/Program.cs:74`

</details>

<details id="CWE-798_Use_of_Hard-coded_Credentials">
<summary><b>CWE-798: Use of Hard-coded Credentials</b> — affected files</summary>

- `infra/main.bicep:58`
- `infra/main.bicep:59`
- `infra/main.bicep:67`

</details>

<details id="CVE-2026-106116_ImageSharp_BigTIFF_IFD_count_can_keep_a_decoder_thread_in_a_non-progressing_loop">
<summary><b>CVE-2026-106116: ImageSharp: BigTIFF IFD count can keep a decoder thread in a non-progressing loop</b> — affected files</summary>

- `PhotoAlbum/PhotoAlbum.csproj:16`

</details>

<details id="CVE-2026-106114_ImageSharp_ICC_CLUT_parsing_allocates_from_unvalidated_channel_and_grid_dimensions">
<summary><b>CVE-2026-106114: ImageSharp: ICC CLUT parsing allocates from unvalidated channel and grid dimensions</b> — affected files</summary>

- `PhotoAlbum/PhotoAlbum.csproj:16`

</details>

---

## Codebase Insights

> **Note:** These documents are generated by AI and may contain inaccuracies or incomplete information. Please review carefully.

> **Codebase Insights aren't available yet.**
>
> These documents are generated when assessment runs with **Full analysis** coverage. Re-run the assessment and set `analysisCoverage: full` to enable them.

[Share feedback](https://aka.ms/ghcp-appmod/feedback)
