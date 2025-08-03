# Maroon

A data-storage library intended for offline, peer-to-peer, occasionally-connected machines, using sync-friendly file storage methods. Data sync is delegated to third-party services like Dropbox or OneDrive.

Maroon will write data to one file per machine, and keeps a history of edits as "revisions", so that any other machine can write a new revision of a record without creating conflicting file updates.
If apps are working from an old version of another machine's history, there may be gaps in the revision chain, or an old revision may be mistakenly used as the basis for an edit. As such, there is also a
mechanism for merging conflicting revisions together into a new revision.

## History

- 2019-03-21: Build 0.1.2.1 
	- Better conflict detection.
	- Fixed path use in SharpListSerialiser.
- 2019-03-27: Build 0.2.0.3
	- Previous revision GUIDs rather than integers.
	- Patched to fix conflict resolution.
	- Patched to fix disk file management.
	- Patched to optimise conflict detection algorithm.
- 2019-06-03: Build 0.2.0.4 
	- Patched to fix null reference in NoteDiskMapper.
- 2019-06-13: Build 0.2.1.1
	- ImportHash field.
	- Fix for null reference in ActionItemDiskMapper.
- 2019-06-27: Build 0.2.2
	- Fixed ActionItemDiskMapper to leave files unchanged during read. 
	- Updated MergeDiskRepository handling of unsaved changes.
- 2020-09-14: Build 0.3.0.1
	- Patch: Fixed OriginXmlSerialiser to read data on disk before saving a list of updates.
- 2020-09-15: Build 0.3.0.2
	- Fixed OriginXmlSerialiser to save a dictionary.
- 2020-10-29: Build 0.4.0.0
	- Added IDiskMapper interface for mappers to specify file name on every operation.
	- Updated MergeDiskRepository and OriginDiskRepository to use one IDiskMapper instance for main and change storage.
- 2021-09-09: Build 0.4.3
	- Fixed lazy-load in SingleOriginRepository.
