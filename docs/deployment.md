# 官网部署与域名

源码仓库：[kingsjoys/kingsSafe](https://github.com/kingsjoys/kingsSafe)。官网只发布 `www/dist/` 构建产物，源码、研究样本和项目内部路线图不会被复制为网站页面。

## GitHub Pages

官网使用 [safe.kingsjoys.com](https://safe.kingsjoys.com/)。原始 Pages 地址为 [kingsjoys.github.io/kingsSafe/](https://kingsjoys.github.io/kingsSafe/)，由 GitHub Pages 管理自定义域名重定向。组织名决定 `kingsjoys.github.io`，仓库名决定 `/kingsSafe/` 路径。

仓库 Settings → Pages 的 Source 使用 GitHub Actions。`.github/workflows/pages.yml` 在主分支相关文件变化或手动触发时构建、检查并发布：

1. 使用 Node.js 构建 `www/`，无需安装前端依赖。
2. 静态检查通过后，只上传 `www/dist/`。
3. 官方 Pages 部署动作发布到 `github-pages` 环境。

PR 只运行 CI，不发布网站。网站链接使用相对路径，支持默认仓库路径及自定义域名根路径。官网公开技术参考采用明确清单；完整设计与研究在 GitHub 仓库阅读。

## 自定义二级域名

官网自定义域名为 `safe.kingsjoys.com`，DNS 的 `safe` CNAME 指向 `kingsjoys.github.io`。绑定与维护步骤依据 [GitHub 官方文档](https://docs.github.com/en/pages/configuring-a-custom-domain-for-your-github-pages-site/managing-a-custom-domain-for-your-github-pages-site)：

1. 在仓库 Pages 设置的 Custom domain 填入 `safe.kingsjoys.com`。
2. 在域名服务商处添加以下 DNS 记录，不修改现有主域网站记录。

| 类型 | 主机记录 | 目标 |
| --- | --- | --- |
| CNAME | safe | kingsjoys.github.io |

目标不带 `https://`、仓库名或路径。使用 GitHub Actions 部署时，域名在 Pages 设置中管理，不依赖构建产物中的 CNAME 文件。

DNS 生效后由 GitHub 自动申请与维护 TLS 证书，证书就绪后启用 Enforce HTTPS。首页 canonical、og:url、仓库官网链接统一使用 `https://safe.kingsjoys.com/`。证书签发与 DNS 传播可能需要等待，按实际 Pages 设置和 HTTPS 访问结果核验。

域名管理员管理 DNS，仓库管理员管理 Pages。绑定状态与 DNS 解析都应核验；有域名验证条件时，可按 GitHub 官方流程验证域名所有权。

## 本地预览

```sh
npm --prefix www run build
npm --prefix www run check
npm --prefix www run dev
```

开发服务器默认使用 `http://127.0.0.1:4173/`，不会触发远程部署。主分支推送才按 Pages 工作流发布。
