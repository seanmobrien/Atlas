﻿# Changelog
Changelog for Atlas as a product: it will cover functional and algorithmic changes that affect Atlas as a whole.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Versioning
Product version is represented by the version tag of the Functions.PublicApi project.
The project version will be appropriately incremented with each change to the product, and the nature of the change logged below.

## Versions

### 1.6.0

#### Blob Storage
* Bug fix: Stopped excessive number of `CreateContainer` API calls being made during blob download.

#### Search
* Ensure all failed search requests are reported as completed by routing dead-lettered search request messages to the `search-results-ready` topic, with the appropriate failure information.
* Added performance logs for initial search.
* Bug fix: all search-related logs are now being tagged with the search request ID to allow complete end-to-end tracking of a search request via Application Insights.
* Added matching request start time to result set.

##### Auto-Heal
* Auto-Heal is disabled on matching algorithm function app

##### Writing of Search Results
* Added an ability to save search results in multiple files. Result files will now be split into two types:
  * Search summary - this will be a single file containing all search result metadata, e.g., number of matches, the original search request, etc.
  * Search results - these will be written to 1 or more files, each containing a maximum number of results (this is a configurable setting AzureStorage:BatchSize).
* Search results will still be written to one file along with search summary if batch size (`SearchResultsBatchSize`) is set to a value less than or equal to '0'. 
  - Default value for 'SearchResultsBatchSize' is '0'.

#### Repeat Search
* Bug fix: the `StoreOriginalSearchResults` function can now successfully process failed searches and successful searches with no matching donors.
* Ability to save search results in multiple files ([see Search section](#writing-of-search-results) for further info).

#### Scoring
* Bug fix: DPB1 match category is now being calculated when the locus typing contained a single null allele, by treating the locus as homozygous for the expressing allele.
* Scoring feature now also calculates whether a position is antigen matched or not.

#### Donor Import
* Added new function `CheckDonorIdsFromFile` that does symmetric check of donors absence/presence in Atlas storage
* Added new function `CheckDonorInfoFromFile` that compares donor/CBU fields with Atlas
* Updated `ImportDonorFile` function to log invalid donor updates to AI if donor is not present in Atlas storage instead of throwing error
* Updated `ImportDonorFile` function to log invalid donor creates for `diff` update mode to AI if donor is present in Atlas storage instead of throwing error
* Tagged donor import logs with the donor import file name.

#### Match Prediction
* Changed the way match prediction requests are queued for processing by activity functions, to prevent search requests with many donors from blocking the completion of smaller search requests.
* Bug fix: Locus `PositionalMatchCategories` are now re-orientated in line with scoring results.

#### Manual Testing
- Locally-running functions added to `Atlas.ManualTesting.Functions.WmdaConsensusDatasetFunctions` to allow running of exercises 1 and 2 of the WMDA consensus dataset.
- Locally-running function added to `Atlas.ManualTesting.Functions.HaplotypeFrequencySetFunctions` to transform haplotype frequency dataset files that failed import due to the presence of an invalid typing.
- Locally-running function added to `Atlas.ManualTesting.Functions.SearchOutcomesFunctions` that retrives search performance and failure information via peeking of the search results notifications topic specified in app settings.

### 1.5.0

#### API Documentation
* Added new http function that generates a JSON schema for the requested ResultSet client model.

#### Deployment & Integration
* Atlas client models and donor import schema now published as NuGet packages to simplify task of integration.
  * Build pipeline extended with tasks for generating NuGet packages.
  * Donor import file schema and "Common" models moved to new, standalone projects so they can be published as packages.

#### Donor Import
* Bug fix: New `DonorImport` function added to publish the donor update messages that keep the matching algorithm donor store in sync with the donor import donor store. This is to prevent messages from being lost if the app restarts during donor import. A second timer function cleans up expired updates to keep the update repository from getting too large.
* Donor import validation errors now logged as custom events to Application Insights.

#### Manual Testing
* New projects have been added to permit the validation of the match prediction algorithm using an externally generated dataset.

#### Matching Algorithm
* Fixed bug where, in certain cases, potential and exact match counts per donor were not being calculated correctly.
* Matching algorithm results notification extended with failure information, including the number of times a failed search has been attempted thus far, and how many attempts remain.

#### Match Prediction
* New endpoint added that allows match prediction to be performed without running a full search. It accepts batches of match prediction requests: one patient vs. a set of donors. Results are written out to blob storage, and a notification sent to a new topic: `match-prediction-results`.
* Fix for bug where predictive match categories were not being consistently applied to different loci, despite them having the same match probability percentage values.

#### Search
* Search results notification has been extended with failure information, including:
  * the stage of failure,
  * the number of times matching has been attempted and how many attempts remain,
  * and whether search as a whole will be retried.

#### Support
* Decreased Time To Live on `audit` service bus subscriptions to avoid topic maximum size limit being reached due to old messages not being cleared. The value has been set to 14 days, which should be enough time for debug/support purposes.

### 1.4.2

#### HLA Metadata Dictionary
* Fix for bug where HMD lookup failed for a decoded MAC that included a deleted allele with an expression suffix.

### 1.4.1

#### Deployment
* Fixes made to build pipelines and terraform files.

### 1.4.0

#### Matching Algorithm

* New Batch scoring endpoint added, to allow standalone scoring feature to be run on multiple donors at once
* Performance improvements greatly improve speed of 1-3 mismatch searches, particularly in very large installations

### 1.3.0

#### Technical 

* Framework updated from .det core 3.1 to .net 6.0.
* Azure functions SDK updated from v3 to v4.

#### Donor Import

- New "changeType" supported for donor import files = `NU` = Upsert ("new or update") - allowing a consumer to provide a donor that should be added or updated, without caring whether that donor was already tracked by Atlas. 
* New config settings added to allow disabling of notifications when: 
  * File successfully imported
  * Donor deletions were attempted for donors that were not tracked in Atlas

#### Matching Algorithm

* Bug fixed where overall match confidence could be assigned "Permissive Mismatch" when non-DPB1 mismatches were known to be present

#### Match Prediction

* Major performance improvements have significantly reduced the time taken for match prediction with large haplotype frequency sets
* Bug fixed where some haplotypes were included twice in the probability calculations

#### MAC Dictionary

* Alerts are now sent when the MAC dictionary import fails

#### Search 

* Atlas can now filter donor results based on registry codes
* Dpb1 Mismatch Direction is now returned in Scoring results from searches.
* Search result now contains details about the search criteria used to initiate the search.
* Search result now contains details about the Haplotype Frequency sets used for match prediction, for both patient and donor results.

### 1.2.0

- Fixed scoring issue in which some DPB1 pairs were erroneously classified as a Non-Permissive Mismatch, when in reality they should be Permissive.
- `PermissiveMismatch` match grade has been removed and will no longer be assigned - see [Client Changelog](../Atlas.Client.Models/CHANGELOG_Client.md) for more details on this change. 

### 1.1.1

- All enum values will now be serialised to strings, to allow ease of parsing the serialised results files / http responses for external consumers, and for human-readability.

### 1.1.0

#### Changed
- Matching and Match Prediction algorithms are now able to run at different HLA nomenclature versions.
  - MPA will now use the HLA versions of the haplotype frequency sets referenced during match probability calculations.
  - Matching will continue to use the HLA version that was set at the time of the last successful data refresh.

### 1.0.1

#### Fixed
- Fix for bug that was preventing HLA metadata dictionary refresh to v3.44.0 of HLA nomenclature.

### 1.0.0

- First stable release of the Atlas product.