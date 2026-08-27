# Contributing

## Branch and pull request flow

1. Branch from `main` using `feature/short-name`, `fix/short-name`, or `chore/short-name`.
2. Keep each pull request focused on one outcome.
3. Add or update tests for behavior changes.
4. Document contract and database changes in the pull request.
5. Request at least one teammate review before merging.

## Definition of done

- `dotnet build` has no warnings.
- `dotnet test` passes.
- New endpoints include appropriate success and error responses.
- Schema changes include an EF Core migration.
- No credentials, personal data, or local database files are committed.

Use conventional commit subjects where practical, for example `feat(catalog): add category filters`.
