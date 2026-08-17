# Fsp Architecture

## Client
Unity client يستهدف Android وiOS وWindows مع طبقة تحكم منفصلة للموبايل والكمبيوتر، وإعدادات جودة قابلة للتدرج.

## Game Simulation
حركة اللاعب، إطلاق النار، الضرر، المركبات، الـSafe Zone وحالة المباراة يجب أن تُدار داخل Game Server/Networking layer وليست عبر Supabase.

## Supabase
يُستخدم للبيانات الدائمة فقط:
- Auth / profiles
- Inventory / cosmetics
- Rank / progression
- Match summary
- Economy metadata

لا تُرسل إليه حركة اللاعب أو الرصاص frame-by-frame.

## Cloudflare
- API gateway / protection
- Edge validation and lightweight endpoints
- Realtime/WebRTC voice for squads
- Rate limiting and abuse protection

## Voice
Squad voice فقط في النسخة الأولى، مع Push-to-Talk وكتم الميكروفون/اللاعبين. لا يوجد proximity/world voice في الـMVP.

## Match Target
- 32 total slots initially
- Humans + bots
- Solo and Squad
- Region-first launch: Middle East

## Content Strategy
- One optimized map at launch
- Addressables / downloadable content for optional future content
- LODs, texture compression, baked lighting where practical, pooled effects and objects

## Scalability Rule
يجب أن تكون خدمات الحسابات والبيانات منفصلة عن Game Server حتى يمكن تغيير مزود الاستضافة أو التوسع دون تحديث كبير للعميل.
