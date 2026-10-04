# Deep Sea Gambler

2026 FGJ 作品。在海底街道上探索，走進建築和怪物玩大話骰，用吹牛和膽量贏回氧氣。

- 線上遊玩：<https://wanderviewer.itch.io/deep-sea-gambler>

## 遊戲玩法

### 操作

- `←` `→` 或 `A` `D`：左右移動
- `↑` 或 `W`：在門口進入建築
- 賭局中用滑鼠點選按鈕、輸入數字，`Enter` 送出喊價，`Tab` 切換欄位

### 目標

街上有 4 棟建築，進去和怪物玩大話骰，打贏最後一棟的烏賊市長就能看到結局。

建築不用照順序挑戰，可以直接衝最後一關。如果從第一關一路打上去，氧氣會延續到下一關，難度會比較高。

### 大話骰規則

- 雙方只看得到自己的骰子
- 喊「N 個 F 點」代表猜全場至少有 N 顆 F 點
- 新的喊價要「數量更多」或「同數量、點數更大」
- 1 點是萬用點，但有人喊 1 點後就失效
- 輪到你時：
  - 「相信」：輸入更大的喊價並送出
  - 「吹牛！開」：開盅，看對方是不是在吹牛
- 開盅後實際數量夠，開盅的人輸；不夠，喊價的人輸

### 氧氣

每輸一局扣 1 點氧氣，先歸零的一方落敗。

- **氧氣會延續到下一關**：打完一棟建築剩多少氧氣，下一棟就從那裡開始
- 打贏怪物會回復 2 點氧氣，上限 5 點
- 輸了可以無限重新挑戰，重來時氧氣會回到進入這棟建築時的數值

### 道具（第 2 關起）

| 道具 | 效果 |
|---|---|
| 偷窺鏡片 | 下一局開始時，隨機得知對方某個點數有幾顆 |
| 偷加骰子 | 偷偷幫自己加一顆骰子 |
| 重搖 | 雙方立刻重搖骰子，目前的喊價保留 |
| 封口膠帶 | 對方跳過下一回合 |
| 偷走氧氣 | 如果這次開盅你贏了，對方多扣 1 點氧氣，你回復 1 點 |

## 開發環境

- Unity 6（6000.3.20f1）
- Universal Render Pipeline
- Input System
- Unity Test Framework

### 開啟專案

1. 用 Unity Hub 安裝 `6000.3.20f1`
2. Clone 專案後在 Unity Hub 選擇「Add project from disk」
3. 開啟 `Assets/Scenes/Bootstrap.unity` 後按 Play

### 場景流程

`Bootstrap` → `MainMenu` → `Exploration` → `Gameplay` → `Ending`

### 專案結構

```
Assets/
├── Art/
├── Audio/
├── Prefabs/
├── Scenes/
├── Scripts/
│   ├── Runtime/   執行期程式碼（FGJ.Runtime）
│   └── Editor/    編輯器程式碼（FGJ.Editor）
└── Tests/
    ├── EditMode/  純邏輯測試
    └── PlayMode/  場景與 MonoBehaviour 測試
```

### 測試

在 Unity 中開啟 `Window > General > Test Runner`，分別執行 EditMode 與 PlayMode 測試。Pull Request 會自動在 GitHub Actions 上跑測試。

## 使用授權

本專案的程式碼、美術、音效與所有資產**保留所有權利**。

如果你想使用、修改、轉載或以任何形式引用本專案的任何內容，請**事先告知並取得開發者本人同意**。可以透過 [GitHub Issues](https://github.com/ZhenrongWu/2026-FGJ-Team/issues) 或 GitHub 個人頁 [@ZhenrongWu](https://github.com/ZhenrongWu) 聯繫。
