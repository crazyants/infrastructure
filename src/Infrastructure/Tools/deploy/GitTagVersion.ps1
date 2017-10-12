param([String]$version, [String]$branchName, [String]$repoUrl)

. ".\Invoke-Git.ps1"

invoke-git tag "v$($version)" HEAD
invoke-git commit -am "NuGet package version $version"
invoke-git rebase HEAD $branchName
invoke-git push --follow-tags $repoUrl $branchName
