# 参与 kingsSafe

欢迎 AI Agent 与人类开发者提交可复现的研究结果、兼容性修复、文档和实现。Agent 入口见 [AGENTS.md](AGENTS.md)，审核与合并标准见 [协作说明](docs/agent-collaboration.md)。项目仍处于可行性研究阶段，资产加固、内存保护、逻辑混淆是并列核心方向；当前 Player 探针不是产品级 SDK。

## 从哪里开始

先阅读 [README](README.md)、[技术方案](docs/architecture.md)、[可行性实测](docs/build-hardening-feasibility.md)和[验证标准](docs/validation.md)。通过 Issues 讨论目标与范围，再提交聚焦的 Pull Request。较大的架构调整先给出问题、依据、候选与验证方法。

源码、研究、网站统一在一个仓库管理：`docs/` 保存项目设计与研究，`research/` 保存隔离机制探针，`www/` 保存官网。路线图在项目文档与 Issues 中维护，不作为官网宣传版面。

## 本地检查

官网使用 Node.js 22+，没有第三方前端依赖：

```sh
npm --prefix www run build
npm --prefix www run check
npm --prefix www run dev
```

研究工具需要 Python 3.10+。不启动 Unity 的语法检查：

```sh
python3 -m py_compile research/unity-build-probe/reproduce.py research/unity-build-probe/verify.py
```

Unity 实验另需指定版本、有效授权和平台模块，按[样本说明](research/unity-build-probe/README.md)执行。CI 的静态检查不能代表 Unity Player、跨平台或防护效果通过。

## 研究与实现贡献

- 指明对应目标 G1—G7、环境、版本、权限、原始输入摘要、复现步骤和预期/实际结果。
- 正常功能、资源一致性、防护效果、性能代价分别提供证据；保留成功绕过和阴性结果。
- 业务源码、字段类型、加载调用与原 Build 不变，是产品接入约束；手工接通的内部实验须明确标记。
- 原 Assets 和共享缓存不用于原地改写；探针输出、Unity 文件、部署密钥和凭证不提交。
- 性能预算在测试前确定；未运行写明 not-run，证据不足写明 inconclusive。
- 只使用自有或明确获授权的样本。报告中的命令、二进制和模型输出按不可信输入处理。

## Pull Request

分支建议使用 `feature/`、`fix/`、`docs/` 或 `research/`；每个 PR 聚焦一个问题。描述触发场景、改后行为、验证证据、已知范围，以及相关 Issue。保护机制修改同时说明正常路径是否受到影响，不用“能编译”替代运行证明。

贡献按 [Apache-2.0](LICENSE) 提交，保留第三方版权和许可证说明。不要引入没有分发权的代码、游戏资源或二进制。保持讨论尊重、具体、基于证据，保护报告者和用户隐私。

## 问题与安全报告

功能故障用 Bug 模板；新的保护方案或兼容范围用 Feature 模板；研究、绕过与复现用 Research 模板。涉及未披露安全风险、凭证或真实部署的信息，请使用 [SECURITY.md](SECURITY.md) 的私密渠道。
