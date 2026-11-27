"""Fetch the sibling framework repos a checkout's projects reference, each at the commit bamtk records.

Bam framework repos reference each other as siblings (..\\..\\bam.test\\bam.test\\bam.test.csproj), the
layout bamtk's submodules/ directory gives them. CI has only this repo, so this script rebuilds that
layout next to it. It follows the ProjectReference graph from every project in the checkout, project by
project, so a sibling is fetched only when a project the build needs references it (a sibling's own test
projects are never followed). Each sibling is fetched at the commit bamtk's default branch records for
submodules/<name>, so CI builds against the framework versions bamtk ships, not whatever each repo's
default branch holds today.

Usage: python sibling_layout.py <checkout-dir> [bamtk-ref]
"""
import os
import re
import subprocess
import sys

OWNER_URL = "https://github.com/BryanApellanes/{}.git"
REFERENCE = re.compile(r'<ProjectReference\s+Include="([^"]+)"')


def run(*args, cwd=None):
    result = subprocess.run(args, cwd=cwd, capture_output=True, text=True)
    if result.returncode != 0:
        sys.exit(f"{' '.join(args)} failed ({result.returncode}): {result.stderr.strip()}")
    return result.stdout


def projects_in(directory):
    for root, dirs, files in os.walk(directory):
        dirs[:] = [d for d in dirs if d not in ("bin", "obj", ".git")]
        for name in files:
            if name.endswith(".csproj"):
                yield os.path.join(root, name)


def references_of(project):
    with open(project, encoding="utf-8-sig") as file:
        text = file.read()
    directory = os.path.dirname(project)
    return [os.path.normpath(os.path.join(directory, include.replace("\\", os.sep))) for include in REFERENCE.findall(text)]


def main():
    checkout = os.path.abspath(sys.argv[1])
    bamtk_ref = sys.argv[2] if len(sys.argv) > 2 else "bamtk"
    parent = os.path.dirname(checkout)
    index = os.path.join(parent, ".bamtk-index")
    if not os.path.isdir(index):
        run("git", "clone", "--quiet", "--depth", "1", "--filter=blob:none", "--no-checkout",
            "--branch", bamtk_ref, OWNER_URL.format("bamtk"), index)

    pending = list(projects_in(checkout))
    seen = set()
    while pending:
        project = pending.pop()
        if project in seen:
            continue
        seen.add(project)
        relative = os.path.relpath(project, parent)
        repo = relative.split(os.sep)[0]
        target = os.path.join(parent, repo)
        if not os.path.isdir(target):
            tree = run("git", "ls-tree", "HEAD", "submodules/" + repo, cwd=index).split()
            if len(tree) < 3 or tree[1] != "commit":
                sys.exit(f"{relative} is referenced, but bamtk records no submodule named {repo}; cannot pin it")
            sha = tree[2]
            run("git", "init", "--quiet", target)
            fetched = subprocess.run(["git", "fetch", "--quiet", "--depth", "1", OWNER_URL.format(repo), sha],
                                     cwd=target, capture_output=True, text=True)
            if fetched.returncode != 0:
                sys.exit(f"cannot fetch {repo} @ {sha} for {os.path.relpath(project, parent)}: {fetched.stderr.strip()}. "
                         "This workflow fetches without a token; if the repo is private it must not be in the build graph.")
            run("git", "checkout", "--quiet", "FETCH_HEAD", cwd=target)
            print(f"{repo} @ {sha}")
        if not os.path.isfile(project):
            sys.exit(f"{relative} is referenced but does not exist at the pinned commit")
        pending.extend(references_of(project))


if __name__ == "__main__":
    main()
