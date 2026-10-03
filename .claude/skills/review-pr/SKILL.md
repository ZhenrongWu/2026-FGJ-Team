---
name: review-pr
description: 審核 GitHub Pull Request。當使用者說「review pr 12」、「審核 PR #12」、「/review-pr 12」或貼上 PR 網址時使用。讀取 PR 差異、依本專案規範檢查，整理審核意見後，經使用者確認再送出 GitHub review（Approve / Request changes / Comment）。
argument-hint: <PR 編號或網址>
allowed-tools: Bash(gh pr view:*), Bash(gh pr diff:*), Bash(gh pr checks:*), Bash(gh pr list:*), Bash(gh api:*), Bash(git fetch:*), Bash(git log:*), Bash(git diff:*), Bash(git show:*)
---

# 審核 Pull Request

要審核的 PR：`$ARGUMENTS`

如果沒有給 PR 編號，先執行 `gh pr list` 列出開啟中的 PR，請使用者選一個。

所有回覆與 GitHub 上的審核留言一律使用**繁體中文**。

## 1. 收集資訊

```bash
gh pr view <PR> --json number,title,author,body,baseRefName,headRefName,files,additions,deletions,commits,reviews,url
gh pr diff <PR>
gh pr checks <PR>
```

- 確認目標分支是 `main`。
- 讀 PR 說明，弄清楚這個 PR 想做什麼；說明不清楚也算一個審核意見。
- 差異不足以判斷時，用 `git fetch origin pull/<PR>/head` 後以 `git show FETCH_HEAD:<path>` 讀完整檔案，**不要 checkout**，避免 Unity 編輯器在開啟中被切換分支而重新匯入整個專案。

## 2. 檢查項目

依嚴重程度由高到低：

### 正確性
- 邏輯錯誤、空參考（`NullReferenceException`）、邊界條件、未處理的狀態。
- Unity 生命週期誤用：`Awake`/`Start`/`OnEnable` 順序依賴、在 `Update` 中做昂貴操作（`GetComponent`、`Find`、配置記憶體）、事件訂閱後沒有在 `OnDisable`/`OnDestroy` 取消。
- 協程、`async` 在物件銷毀後仍執行。

### 測試（本專案規範：每個功能都要有測試）
- 新功能或修改行為必須附帶測試：純邏輯放 `Assets/Tests/EditMode`，MonoBehaviour／場景行為放 `Assets/Tests/PlayMode`。
- 沒有測試 → 列為**必須修正**。
- 測試是否真的驗證了行為，而不是只跑過程式碼。

### Unity 專案規範
- **`.meta` 檔**：每個新增／搬移／刪除的資產都要有對應的 `.meta` 變動；缺 `.meta` 或孤立的 `.meta` 都要指出。
- **資料夾結構**：檔案放在正確位置（`Art/`、`Audio/`、`Prefabs/`、`Scenes/`、`Scripts/Runtime`、`Scripts/Editor`、`Tests/`）。
- **組件定義**：執行期程式碼放 `FGJ.Runtime`，編輯器程式碼放 `FGJ.Editor`，執行期程式碼不能 `using UnityEditor`（除非包在 `#if UNITY_EDITOR`）。
- **不該提交的檔案**：`Library/`、`Temp/`、`Logs/`、`UserSettings/`、`*.csproj`、`*.sln`、建置產物。
- **場景／Prefab 衝突風險**：大幅修改 `.unity` 或 `.prefab` 時提醒是否有其他人同時在改同一個檔案。
- `Packages/manifest.json` 有變動時，確認 `packages-lock.json` 也一起更新，且新套件有固定版本。

### Commit 與 PR 格式
- Commit 訊息符合 Conventional Commits（`feat:`、`fix:`、`chore:`…）＋繁體中文。
- PR 範圍單一，不混雜無關修改。

### 可讀性與維護性（次要）
- 命名、命名空間（`FGJ.*`）、重複程式碼、過度複雜的寫法。

## 3. 回報給使用者

用以下格式在對話中整理結果，**先不要送出到 GitHub**：

```
## PR #<編號>：<標題>
作者：<author>　分支：<head> → main　變更：+<additions> / -<deletions>，<n> 個檔案
CI：<通過 / 失敗 / 無>

### 摘要
<一兩句說明這個 PR 做了什麼>

### 🔴 必須修正
- `path/to/file.cs:42` — <問題與原因>，建議：<怎麼改>

### 🟡 建議修改
- ...

### 🟢 小建議（可不改）
- ...

### 建議結論：Approve / Request changes / Comment
```

沒有問題的分類就省略。沒有任何問題時直接說明可以核准。

## 4. 送出審核

詢問使用者要用哪種結論送出（預設採用上面的建議結論），得到確認後才執行：

- 核准：`gh pr review <PR> --approve --body "<審核摘要>"`
- 要求修改：`gh pr review <PR> --request-changes --body "<審核摘要>"`
- 只留言：`gh pr review <PR> --comment --body "<審核摘要>"`

需要針對特定行留言時，使用 review API 一次送出行內留言：

```bash
gh api repos/{owner}/{repo}/pulls/<PR>/reviews --method POST --input review.json
```

`review.json` 內含 `event`（`APPROVE` / `REQUEST_CHANGES` / `COMMENT`）、`body`，以及 `comments` 陣列（每筆包含 `path`、`line`、`side: "RIGHT"`、`body`）。暫存檔放在 scratchpad 目錄。

注意：
- PR 作者是使用者本人時，GitHub 不允許 Approve 或 Request changes，只能用 Comment。
- 不要自動合併 PR；使用者要求合併時才執行 `gh pr merge <PR> --squash`（或使用者指定的方式）。
- 審核留言中不要加任何 Claude 署名。
