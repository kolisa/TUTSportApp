# Contributing to TUTSportApp

Thank you for considering contributing! Please follow these guidelines:

## Running Tests
- Use `dotnet test` to run all unit and integration tests.
- Coverage reports are generated in `TestResults/CoverageReport`.

## Code Style
- Follow .editorconfig and SonarQube rules.
- One assertion per test method.
- Use descriptive test names.

## Pull Requests
- All PRs must pass CI, code coverage, and static analysis.
- Add or update tests for all code changes.
- No direct commits to main; use feature branches.

## Mocking
- Mock all dependencies in unit tests (no real DB, HTTP, or external services).

## Test Data
- Use test data builders or factories for complex objects.

## Linting
- Run static analysis before submitting PRs.

## Questions?
Open an issue or discussion in the repository.
