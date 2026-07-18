# Git / GitHub SSH Setup

Steps to get a new machine (Mac, PC, or VM) authenticating with GitHub over SSH, so `git clone`, `git pull`, and `git push` work without passwords or tokens.

> Background: GitHub removed password auth for Git operations in 2021. HTTPS clones now need a Personal Access Token, or (recommended, since it's set-and-forget) an SSH key.

---

## 1. Generate a new SSH key

Use a key name specific to the machine so you never accidentally overwrite an existing key.

```bash
ssh-keygen -t ed25519 -C "mac-dev" -f ~/.ssh/id_ed25519_mac
```

- Replace `mac-dev` with something identifying the machine, e.g. `work-pc`, `home-vm`.
- Replace the `-f` filename to match, e.g. `~/.ssh/id_ed25519_workpc`.
- When prompted for a passphrase: either set one you'll actually remember and write down, or press Enter twice for no passphrase (simpler day-to-day, slightly less secure if the machine is compromised).

⚠️ **If you forget a passphrase later, there is no recovery** — you just generate a new key and re-register it on GitHub. Don't sink time trying to recall it.

## 2. Copy the public key

```bash
cat ~/.ssh/id_ed25519_mac.pub
```

Copy the full output line (starts `ssh-ed25519 AAAA...`).

## 3. Add it to GitHub

1. GitHub → click your avatar (top right) → **Settings**
2. **SSH and GPG keys** (left sidebar)
3. **New SSH key**
4. **Title**: name it after the machine (e.g. `mac-dev`, `work-pc`)
5. **Key type**: Authentication Key
6. **Key**: paste the public key
7. **Add SSH key**

You can register keys from as many machines as you like against the same account — each just needs a distinct title.

## 4. Configure SSH to use the right key for GitHub

Edit (or create) `~/.ssh/config`:

```bash
nano ~/.ssh/config
```

Add:

```
Host github.com
  AddKeysToAgent yes
  UseKeychain yes
  IdentityFile ~/.ssh/id_ed25519_mac
```

> Note: `UseKeychain yes` is macOS-only (stores the passphrase in Keychain). Omit that line on Windows/Linux.

Save and exit in nano: `Ctrl+O`, `Enter`, `Ctrl+X`.

Update the `IdentityFile` path to match whatever filename you used in step 1.

## 5. Add the key to the agent (macOS)

```bash
ssh-add --apple-use-keychain ~/.ssh/id_ed25519_mac
```

This caches the passphrase so you're not typing it every session.

*(On Windows/Linux, drop `--apple-use-keychain` and just run `ssh-add ~/.ssh/id_ed25519_mac`.)*

## 6. Test it

```bash
ssh -T git@github.com
```

Expected output:

```
Hi ridgeassociates! You've successfully authenticated, but GitHub does not provide shell access.
```

That message is success — GitHub SSH never gives a shell, it's just confirming auth.

## 7. Clone using the SSH URL (not HTTPS)

```bash
git clone git@github.com:RidgeProjects/bingo-board.git
```

---

## Working across Mac + PC

- Each machine gets its **own** SSH keypair — don't copy private keys between machines.
- Register **each** machine's public key separately under the same GitHub account.
- Set line-ending handling to avoid noisy diffs when switching OSes:
  ```bash
  # macOS
  git config --global core.autocrlf input

  # Windows
  git config --global core.autocrlf true
  ```

## Quick troubleshooting

| Symptom | Likely cause |
|---|---|
| `Password authentication is not supported` | You're using an HTTPS URL with a password — switch to SSH URL or use a PAT |
| `zsh: command not found: Host` (or similar) | You pasted the SSH config block into the terminal instead of into `~/.ssh/config` — open the file with `nano ~/.ssh/config` and paste there instead |
| Repeated passphrase prompts | Key isn't cached — run `ssh-add --apple-use-keychain ~/.ssh/<keyfile>` |
| `Permission denied (publickey)` | Public key not added to GitHub, or `~/.ssh/config` pointing at the wrong `IdentityFile` |
