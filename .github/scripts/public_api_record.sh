#!/usr/bin/env bash
# Compares the public API this run extracted with the record committed in the repository. On a difference it lists the
# changed lines in the job summary, copies the extracted API to $RUNNER_TEMP/public-api-record/ under the record's path,
# for the workflow to upload, and fails: a deliberate change is committed together with a changelog line.
#   $@ pairs "<extracted file>=<record path>"
set -euo pipefail
out="$RUNNER_TEMP/public-api-record"
status=0
current="$RUNNER_TEMP/public-api-current.txt"
recorded="$RUNNER_TEMP/public-api-recorded.txt"
for pair in "$@"; do
  extracted=${pair%%=*}
  record=${pair#*=}
  # Line endings do not count: .NET writes CRLF on Windows, and a Windows checkout can give the record CRLF too.
  tr -d '\r' < "$extracted" > "$current"
  if [ -f "$record" ] && tr -d '\r' < "$record" | cmp -s - "$current"; then
    echo "The public API is the one $record records."
    continue
  fi
  status=1
  mkdir -p "$out/$(dirname "$record")"
  cp "$current" "$out/$record"
  {
    printf '### The public API differs from %s\n\n' "$record"
    printf 'Commit `%s` from the `public-api-record` artifact of this run, with a changelog line for the change.\n\n' "$record"
    if [ -f "$record" ]; then
      printf '```diff\n'
      tr -d '\r' < "$record" > "$recorded"
      diff -u "$recorded" "$current" | tail -n +3 | head -n 300 || true
      printf '```\n\n'
    else
      printf 'There is no record yet.\n\n'
    fi
  } >> "$GITHUB_STEP_SUMMARY"
  echo "::error title=Public API record::The public API differs from $record. Commit the file from this run's public-api-record artifact and add a changelog line."
done
exit "$status"
