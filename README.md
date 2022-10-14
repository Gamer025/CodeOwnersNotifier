# CodeOwnersParser
## Github action for parsing the Codeowner file
CodeOwnersParser is an Github action that parses the Github [Codeowner](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners)
file and returns all owners with modfied files in a pull request.

This output can then for example be used to assign these owners to the pull request or notify them via a comment.
This allows you to have codeowners without giving them write access to your repo or them being a team member at all. (Assignees must be a team memember but require no write access)

### Inputs
CodeOwnersParser takes the following inputs:
Name | Required | Default | Description
------------ | ------------- | ------------- | -------------
owner | no | github.repository_owner | Name of repo owner
name | no | github.repository | Name of the repo
timeout | no | '10000' | Timeout for Github API calls in milliseconds.
fileLimit | no | '1000' | Maximum amount of file a PR might have, if exceeded it won't be processed. Setting this to a high value might result in rate limiting.
token | no | github.token | Token used for Github API calls. Only needed to increase rate limits, can be replaced with empty string, but might lead to rate limit errors.
workspace | no | '/github/workspace' | Location of the repo in the workflow. Normally '/github/workspace' for Docker workflows.
file | no | '/.github/CODEOWNERS' | Relative location of the CODEOWNERS file in the repo. Useful if you want a separate owner file for this action.
pullID | no | github.event.pull_request.number | ID of the PR to get modified files from
separator | no | ' ' | Separator used for owners-formatted output and for finding already notified owners.
prefix | no | "" | Text to add infront of the owners list in owners-formatted
sufix | no | "" | Text to add after the the owners list in owners-formatted
botname | no | No default | Name of the bot notifying owners. Set this if you use the output to compose comments on the PR. Set this to your bot name (or github-actions[bot]) so that already notified owners will be found and not output. Make sure prefix and sufix is correct.

### Outputs
Name | Description
------------ | -------------
owners | Raw list of owners, separated with the seperator input, to be used to check if there are any found owners or as input for other steps.
owners-formatted | Owners with prefix and suffix added, to be used as input for a comment posting action
