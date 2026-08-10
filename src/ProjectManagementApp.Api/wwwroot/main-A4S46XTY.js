import{$n as ls,A as GE,Ar as vm,At as Ue,B as IE,Bn as hr,Bt as WE,C as Ei$1,Cr as ts,Dn as ey,Dr as us,Dt as TD,Et as T$1,Fn as gg,Hn as ie$2,Hr as zF,Ht as Wc,I as Hc,In as gm,Ir as xe,It as Vp,J as Jl,K as JE,Kn as jn$1,Kr as l,Kt as Wp,Ln as gr,N as Gh,Nr as wE,On as fD,Ot as UE,Pn as ge,Q as Kp,Qn as le$1,Qt as Yp,R as Hp,Rn as gt$2,Rr as yE,S as Eh,St as Rp,T as Er,Tn as e1,Tt as Sg,Un as it$2,V as Ig,Vn as iD,Vr as z$1,W as Ir,Wn as j$2,Wr as zp,Wt as Wl,Xn as ki$1,Xt as Xp,Y as Jp,Yn as kg,Zt as YF,_ as Cn,_n as cD,_t as Q$1,a as $l,ar as n1,br as t1,c as A$2,cn as _u,cr as nw,d as Bc,dt as Og,en as Zu,er as lu,et as LI,f as Bl,fr as pD,ft as Oi$1,gn as bu,gr as ql,h as Bv,ht as Pg,i as $e,in as _o,ir as mv,it as M$1,jn as fs,jt as Ul,kr as vg,l as Ae$1,ln as aD,lr as or,m as Bp,mt as PD,n as $a,nr as mg,nt as Lm,o as $o,p as Bn,pn as as,pr as qD,pt as Op,q as JF,qr as m,qt as XD,r as $c,rn as _g,rt as Ln$1,s as $p,sn as _t$2,sr as nh,t as $E,tn as _,tt as Lg,u as Ag,un as ah,ut as Ng,v as Dg,vn as ce$1,vr as rg,vt as QF,wr as uD,wt as Se,x as Eg,xn as dD,xt as Rg,yr as ss,zr as yg}from"./chunk-BdyQRKj_.js";import{a as Js,c as Qn$1,d as Ye$2,f as _c,i as Ha,l as Tr,m as ii$1,n as Da,o as Lo,p as bc,r as Ga,s as Ne,t as $a$1,u as Ue$1,y as za}from"./chunk-C6lNe7c4.js";import{_ as w,a as K$3,c as Te$1,d as X$2,f as bn,g as vn,h as rt$3,i as J$4,l as W$3,m as mn,n as E,o as R$1,p as gn,r as In,s as Rn,t as D$1,u as We,v as xe$1,y as ye}from"./chunk-EsxVdIKO.js";import{A as si$1,C as ei$1,D as mn$1,I as y$1,L as yt$1,N as tr,O as p$1,R as z$2,S as di$1,T as je$1,_ as Zn$1,a as D$2,d as Pt$1,g as Y$2,h as Wo,i as Ct$1,l as Oe$1,n as A$3,o as In$1,t as $o$1,u as Ot$1,v as Zo,w as er}from"./chunk-CgWjumkv.js";var n=class t{http=T$1(Lo);register(e){return this.http.post(`/api/auth/register`,e)}login(e){return this.http.post(`/api/auth/login`,e,{withCredentials:!0})}logout(){return this.http.post(`/api/auth/logout`,null,{withCredentials:!0})}refresh(){return this.http.post(`/api/auth/refresh`,null,{withCredentials:!0})}static ɵfac=function(i){return new(i||t)};static ɵprov=ie$2({token:t,factory:t.ɵfac,providedIn:`root`})};var K$2={dispatch:!0,functional:!1,useEffectsErrorHandler:!0};var h=`__@ngrx/effects_create__`;function Ct(t,r={}){let e=r.functional?t:t(),n=l(l({},K$2),r);return Object.defineProperty(e,h,{value:n}),e}function Y$1(t){return Object.getOwnPropertyNames(t).filter(n=>t[n]&&t[n].hasOwnProperty(h)?t[n][h].hasOwnProperty(`dispatch`):!1).map(n=>{let s=t[n][h];return l({propertyName:n},s)})}function J$3(t){return Y$1(t)}function U$2(t){return Object.getPrototypeOf(t)}function L$1(t){return!!t.constructor&&t.constructor.name!==`Object`&&t.constructor.name!==`Function`}function G$3(t){return typeof t==`function`}function X$1(t){return t.filter(G$3)}function q$2(t,r,e){let n=U$2(t),o=!!n&&n.constructor.name!==`Object`?n.constructor.name:null;return mg(...J$3(t).map(({propertyName:i,dispatch:V,useEffectsErrorHandler:B})=>{let S=typeof t[i]==`function`?t[i]():t[i],M=B?e(S,r):S;return V===!1?M.pipe(Eg()):M.pipe(Ng()).pipe(ce$1(z=>({effect:t[i],notification:z,propertyName:i,sourceName:o,sourceInstance:t})))}))}var Q=10;function H(t,r,e=Q){return t.pipe(ls(n=>(r&&r.handleError(n),e<=1?t:H(t,r,e-1))))}var bt$1=(()=>{class t extends _{constructor(e){super(),e&&(this.source=e)}lift(e){let n=new t;return n.source=this,n.operator=e,n}static{this.ɵfac=function(n){return new(n||t)(Se(J$4))}}static{this.ɵprov=ie$2({token:t,factory:t.ɵfac,providedIn:`root`})}}return t})();function Dt$1(...t){return Bn(r=>t.some(e=>typeof e==`string`?e===r.type:e.type===r.type))}var W$2=new A$2(`@ngrx/effects Effects Error Handler`,{providedIn:`root`,factory:()=>H});var tt$3=We(`@ngrx/effects/init`);function et$2(t,r){if(t.notification.kind===`N`){let e=t.notification.value;!nt$2(e)&&r.handleError(new Error(`Effect ${rt$2(t)} dispatched an invalid action: ${ot$2(e)}`))}}function nt$2(t){return typeof t!=`function`&&t&&t.type&&typeof t.type==`string`}function rt$2({propertyName:t,sourceInstance:r,sourceName:e}){let n=typeof r[t]==`function`;return!!e?`"${e}.${String(t)}${n?`()`:``}"`:`"${String(t)}()"`}function ot$2(t){try{return JSON.stringify(t)}catch{return t}}var st$1=`ngrxOnIdentifyEffects`;function it$1(t){return y(t,st$1)}var ft$3=`ngrxOnRunEffects`;function ct$2(t){return y(t,ft$3)}var ut$3=`ngrxOnInitEffects`;function at$2(t){return y(t,ut$3)}function y(t,r){return t&&r in t&&typeof t[r]==`function`}var k=(()=>{class t extends z$1{constructor(e,n){super(),this.errorHandler=e,this.effectsErrorHandler=n}addEffects(e){this.next(e)}toActions(){return this.pipe(_g(e=>L$1(e)?U$2(e):e),_t$2(e=>e.pipe(_g(dt$2))),_t$2(e=>{return mg(e.pipe(Bl(o=>lt$2(this.errorHandler,this.effectsErrorHandler)(o)),ce$1(o=>(et$2(o,this.errorHandler),o.notification)),Bn(o=>o.kind===`N`&&o.value!=null),Dg()),e.pipe(us(1),Bn(at$2),ce$1(o=>o.ngrxOnInitEffects())))}))}static{this.ɵfac=function(n){return new(n||t)(Se(it$2),Se(W$2))}}static{this.ɵprov=ie$2({token:t,factory:t.ɵfac,providedIn:`root`})}}return t})();function dt$2(t){return it$1(t)?t.ngrxOnIdentifyEffects():``}function lt$2(t,r){return e=>{let n=q$2(e,t,r);return ct$2(e)?e.ngrxOnRunEffects(n):n}}var Et=(()=>{class t{get isStarted(){return!!this.effectsSubscription}constructor(e,n){this.effectSources=e,this.store=n,this.effectsSubscription=null}start(){this.effectsSubscription||(this.effectsSubscription=this.effectSources.toActions().subscribe(this.store))}ngOnDestroy(){this.effectsSubscription&&(this.effectsSubscription.unsubscribe(),this.effectsSubscription=null)}static{this.ɵfac=function(n){return new(n||t)(Se(k),Se(X$2))}}static{this.ɵprov=ie$2({token:t,factory:t.ɵfac,providedIn:`root`})}}return t})();function wt$1(...t){let r=t.flat();return _o([X$1(r),lu(()=>{T$1(K$3),T$1(xe$1,{optional:!0});let n=T$1(Et),s=T$1(k),o=!n.isStarted;o&&n.start();for(let c of r){let i=G$3(c)?T$1(c):c;s.addEffects(i)}o&&T$1(X$2).dispatch(tt$3())})])}var o={accessToken:null,expiresAt:null,user:null,isLoading:!1,error:null};var f=mn({name:`auth`,reducer:bn(o,Rn(In.loginSubmitted,r=>m(l({},r),{isLoading:!0,error:null})),Rn(In.loginSuccess,In.tokenRefreshed,(r,{accessToken:l$1,expiresAt:u,user:c})=>m(l({},r),{isLoading:!1,accessToken:l$1,expiresAt:u,user:c,error:null})),Rn(In.loginFailure,(r,{error:l$2})=>m(l({},r),{isLoading:!1,error:l$2})),Rn(In.sessionCleared,In.logoutCompleted,()=>o))});var p=()=>{let c=T$1(X$2),n=T$1(Ue$1);return c.selectSignal(f.selectAccessToken)()?!0:n.createUrlTree([`/auth/login`])};function V$1(i){return Error(`Unable to find icon with the name "${i}"`)}function X(){return Error(`Could not find HttpClient for use with Angular Material icons. Please add provideHttpClient() to your providers.`)}function q$1(i){return Error(`The URL provided to MatIconRegistry was not trusted as a resource URL via Angular's DomSanitizer. Attempted URL was "${i}".`)}function Y(i){return Error(`The literal provided to MatIconRegistry was not trusted as safe HTML by Angular's DomSanitizer. Attempted literal was "${i}".`)}var a=class{url;svgText;options;svgElement=null;constructor(l,t,e){this.url=l,this.svgText=t,this.options=e}};var K$1=(()=>{class i{_httpClient;_sanitizer;_errorHandler;_document;_svgIconConfigs=new Map;_iconSetConfigs=new Map;_cachedIconsByUrl=new Map;_inProgressUrlFetches=new Map;_fontCssClassesByAlias=new Map;_resolvers=[];_defaultFontSetClass=[`material-icons`,`mat-ligature-font`];constructor(t,e,n,o){this._httpClient=t,this._sanitizer=e,this._errorHandler=o,this._document=n}addSvgIcon(t,e,n){return this.addSvgIconInNamespace(``,t,e,n)}addSvgIconLiteral(t,e,n){return this.addSvgIconLiteralInNamespace(``,t,e,n)}addSvgIconInNamespace(t,e,n,o){return this._addSvgIconConfig(t,e,new a(n,null,o))}addSvgIconResolver(t){return this._resolvers.push(t),this}addSvgIconLiteralInNamespace(t,e,n,o){let r=this._sanitizer.sanitize(Q$1.HTML,n);if(!r)throw Y(n);let s=mn$1(r);return this._addSvgIconConfig(t,e,new a(``,s,o))}addSvgIconSet(t,e){return this.addSvgIconSetInNamespace(``,t,e)}addSvgIconSetLiteral(t,e){return this.addSvgIconSetLiteralInNamespace(``,t,e)}addSvgIconSetInNamespace(t,e,n){return this._addSvgIconSetConfig(t,new a(e,null,n))}addSvgIconSetLiteralInNamespace(t,e,n){let o=this._sanitizer.sanitize(Q$1.HTML,e);if(!o)throw Y(e);let r=mn$1(o);return this._addSvgIconSetConfig(t,new a(``,r,n))}registerFontClassAlias(t,e=t){return this._fontCssClassesByAlias.set(t,e),this}classNameForFontAlias(t){return this._fontCssClassesByAlias.get(t)||t}setDefaultFontSetClass(...t){return this._defaultFontSetClass=t,this}getDefaultFontSetClass(){return this._defaultFontSetClass}getSvgIconFromUrl(t){let e=this._sanitizer.sanitize(Q$1.RESOURCE_URL,t);if(!e)throw q$1(t);let n=this._cachedIconsByUrl.get(e);return n?ss(C(n)):this._loadSvgIconFromConfig(new a(t,null)).pipe(Wl(o=>this._cachedIconsByUrl.set(e,o)),ce$1(o=>C(o)))}getNamedSvgIcon(t,e=``){let n=J$2(e,t),o=this._svgIconConfigs.get(n);if(o)return this._getSvgFromConfig(o);if(o=this._getIconConfigFromResolvers(e,t),o)return this._svgIconConfigs.set(n,o),this._getSvgFromConfig(o);let r=this._iconSetConfigs.get(e);return r?this._getSvgFromIconSetConfigs(t,r):as(V$1(n))}ngOnDestroy(){this._resolvers=[],this._svgIconConfigs.clear(),this._iconSetConfigs.clear(),this._cachedIconsByUrl.clear()}_getSvgFromConfig(t){return t.svgText?ss(C(this._svgElementFromConfig(t))):this._loadSvgIconFromConfig(t).pipe(ce$1(e=>C(e)))}_getSvgFromIconSetConfigs(t,e){let n=this._extractIconWithNameFromAnySet(t,e);if(n)return ss(n);return gg(e.filter(r=>!r.svgText).map(r=>this._loadSvgIconSetFromConfig(r).pipe(ls(s=>{let f=`Loading icon set URL: ${this._sanitizer.sanitize(Q$1.RESOURCE_URL,r.url)} failed: ${s.message}`;return this._errorHandler.handleError(new Error(f)),ss(null)})))).pipe(ce$1(()=>{let r=this._extractIconWithNameFromAnySet(t,e);if(!r)throw V$1(t);return r}))}_extractIconWithNameFromAnySet(t,e){for(let n=e.length-1;n>=0;n--){let o=e[n];if(o.svgText&&o.svgText.toString().indexOf(t)>-1){let r=this._svgElementFromConfig(o),s=this._extractSvgIconFromSet(r,t,o.options);if(s)return s}}return null}_loadSvgIconFromConfig(t){return this._fetchIcon(t).pipe(Wl(e=>t.svgText=e),ce$1(()=>this._svgElementFromConfig(t)))}_loadSvgIconSetFromConfig(t){return t.svgText?ss(null):this._fetchIcon(t).pipe(Wl(e=>t.svgText=e))}_extractSvgIconFromSet(t,e,n){let o=t.querySelector(`[id="${e}"]`);if(!o)return null;let r=o.cloneNode(!0);if(r.removeAttribute(`id`),r.nodeName.toLowerCase()===`svg`)return this._setSvgAttributes(r,n);if(r.nodeName.toLowerCase()===`symbol`)return this._setSvgAttributes(this._toSvgElement(r),n);let s=this._svgElementFromString(mn$1(`<svg></svg>`));return s.appendChild(r),this._setSvgAttributes(s,n)}_svgElementFromString(t){let e=this._document.createElement(`DIV`);e.innerHTML=t;let n=e.querySelector(`svg`);if(!n)throw Error(`<svg> tag not found`);return n}_toSvgElement(t){let e=this._svgElementFromString(mn$1(`<svg></svg>`)),n=t.attributes;for(let o=0;o<n.length;o++){let{name:r,value:s}=n[o];r!==`id`&&e.setAttribute(r,s)}for(let o=0;o<t.childNodes.length;o++)t.childNodes[o].nodeType===this._document.ELEMENT_NODE&&e.appendChild(t.childNodes[o].cloneNode(!0));return e}_setSvgAttributes(t,e){return t.setAttribute(`fit`,``),t.setAttribute(`height`,`100%`),t.setAttribute(`width`,`100%`),t.setAttribute(`preserveAspectRatio`,`xMidYMid meet`),t.setAttribute(`focusable`,`false`),e&&e.viewBox&&t.setAttribute(`viewBox`,e.viewBox),t}_fetchIcon(t){let{url:e,options:n}=t,o=n?.withCredentials??!1;if(!this._httpClient)throw X();if(e==null)throw Error(`Cannot fetch icon from URL "${e}".`);let r=this._sanitizer.sanitize(Q$1.RESOURCE_URL,e);if(!r)throw q$1(e);let s=this._inProgressUrlFetches.get(r);if(s)return s;let h=this._httpClient.get(r,{responseType:`text`,withCredentials:o}).pipe(ce$1(f=>mn$1(f)),$l(()=>this._inProgressUrlFetches.delete(r)),fs());return this._inProgressUrlFetches.set(r,h),h}_addSvgIconConfig(t,e,n){return this._svgIconConfigs.set(J$2(t,e),n),this}_addSvgIconSetConfig(t,e){let n=this._iconSetConfigs.get(t);return n?n.push(e):this._iconSetConfigs.set(t,[e]),this}_svgElementFromConfig(t){if(!t.svgElement){let e=this._svgElementFromString(t.svgText);this._setSvgAttributes(e,t.options),t.svgElement=e}return t.svgElement}_getIconConfigFromResolvers(t,e){for(let n=0;n<this._resolvers.length;n++){let o=this._resolvers[n](e,t);if(o)return Z$1(o)?new a(o.url,null,o.options):new a(o,null)}}static ɵfac=function(e){return new(e||i)(Se(Lo,8),Se(Ga),Se(or,8),Se(it$2))};static ɵprov=ie$2({token:i,factory:i.ɵfac,providedIn:`root`})}return i})();function C(i){return i.cloneNode(!0)}function J$2(i,l){return i+`:`+l}function Z$1(i){return!!(i.url&&i.options)}var tt$2=[`*`];var et$1=new A$2(`MAT_ICON_DEFAULT_OPTIONS`);var nt$1=new A$2(`mat-icon-location`,{providedIn:`root`,factory:()=>{let i=T$1(or),l=i?i.location:null;return{getPathname:()=>l?l.pathname+l.search:``}}});var G$2=[`clip-path`,`color-profile`,`src`,`cursor`,`fill`,`filter`,`marker`,`marker-start`,`marker-mid`,`marker-end`,`mask`,`stroke`];var ot$1=G$2.map(i=>`[${i}]`).join(`, `);var rt$1=/^url\(['"]?#(.*?)['"]?\)$/;var wt=(()=>{class i{_elementRef=T$1(Er);_iconRegistry=T$1(K$1);_location=T$1(nt$1);_errorHandler=T$1(it$2);_defaultColor;get color(){return this._color||this._defaultColor}set color(t){this._color=t}_color;inline=!1;get svgIcon(){return this._svgIcon}set svgIcon(t){t!==this._svgIcon&&(t?this._updateSvgIcon(t):this._svgIcon&&this._clearSvgElement(),this._svgIcon=t)}_svgIcon;get fontSet(){return this._fontSet}set fontSet(t){let e=this._cleanupFontValue(t);e!==this._fontSet&&(this._fontSet=e,this._updateFontIconClasses())}_fontSet;get fontIcon(){return this._fontIcon}set fontIcon(t){let e=this._cleanupFontValue(t);e!==this._fontIcon&&(this._fontIcon=e,this._updateFontIconClasses())}_fontIcon;_previousFontSetClass=[];_previousFontIconClass;_svgName=null;_svgNamespace=null;_previousPath;_elementsWithExternalReferences;_currentIconFetch=j$2.EMPTY;constructor(){let t=T$1(new Eh(`aria-hidden`),{optional:!0}),e=T$1(et$1,{optional:!0});e&&(e.color&&(this.color=this._defaultColor=e.color),e.fontSet&&(this.fontSet=e.fontSet)),t||this._elementRef.nativeElement.setAttribute(`aria-hidden`,`true`)}_splitIconName(t){if(!t)return[``,``];let e=t.split(`:`);switch(e.length){case 1:return[``,e[0]];case 2:return e;default:throw Error(`Invalid icon name: "${t}"`)}}ngOnInit(){this._updateFontIconClasses()}ngAfterViewChecked(){let t=this._elementsWithExternalReferences;if(t&&t.size){let e=this._location.getPathname();e!==this._previousPath&&(this._previousPath=e,this._prependPathToReferences(e))}}ngOnDestroy(){this._currentIconFetch.unsubscribe(),this._elementsWithExternalReferences&&this._elementsWithExternalReferences.clear()}_usingFontIcon(){return!this.svgIcon}_setSvgElement(t){this._clearSvgElement();let e=this._location.getPathname();this._previousPath=e,this._cacheChildrenWithExternalReferences(t),this._prependPathToReferences(e),this._elementRef.nativeElement.appendChild(t)}_clearSvgElement(){let t=this._elementRef.nativeElement,e=t.childNodes.length;for(this._elementsWithExternalReferences&&this._elementsWithExternalReferences.clear();e--;){let n=t.childNodes[e];(n.nodeType!==1||n.nodeName.toLowerCase()===`svg`)&&n.remove()}}_updateFontIconClasses(){if(!this._usingFontIcon())return;let t=this._elementRef.nativeElement,e=(this.fontSet?this._iconRegistry.classNameForFontAlias(this.fontSet).split(/ +/):this._iconRegistry.getDefaultFontSetClass()).filter(n=>n.length>0);this._previousFontSetClass.forEach(n=>t.classList.remove(n)),e.forEach(n=>t.classList.add(n)),this._previousFontSetClass=e,this.fontIcon!==this._previousFontIconClass&&!e.includes(`mat-ligature-font`)&&(this._previousFontIconClass&&t.classList.remove(this._previousFontIconClass),this.fontIcon&&t.classList.add(this.fontIcon),this._previousFontIconClass=this.fontIcon)}_cleanupFontValue(t){return typeof t==`string`?t.trim().split(` `)[0]:t}_prependPathToReferences(t){let e=this._elementsWithExternalReferences;e&&e.forEach((n,o)=>{n.forEach(r=>{o.setAttribute(r.name,`url('${t}#${r.value}')`)})})}_cacheChildrenWithExternalReferences(t){let e=t.querySelectorAll(ot$1),n=this._elementsWithExternalReferences=this._elementsWithExternalReferences||new Map;for(let o=0;o<e.length;o++)G$2.forEach(r=>{let s=e[o],h=s.getAttribute(r),f=h?h.match(rt$1):null;if(f){let p=n.get(s);p||(p=[],n.set(s,p)),p.push({name:r,value:f[1]})}})}_updateSvgIcon(t){if(this._svgNamespace=null,this._svgName=null,this._currentIconFetch.unsubscribe(),t){let[e,n]=this._splitIconName(t);e&&(this._svgNamespace=e),n&&(this._svgName=n),this._currentIconFetch=this._iconRegistry.getNamedSvgIcon(n,e).pipe(us(1)).subscribe(o=>this._setSvgElement(o),o=>{let r=`Error retrieving icon ${e}:${n}! ${o.message}`;this._errorHandler.handleError(new Error(r))})}}static ɵfac=function(e){return new(e||i)};static ɵcmp=yE({type:i,selectors:[[`mat-icon`]],hostAttrs:[`role`,`img`,1,`mat-icon`,`notranslate`],hostVars:10,hostBindings:function(e,n){e&2&&(Vp(`data-mat-icon-type`,n._usingFontIcon()?`font`:`svg`)(`data-mat-icon-name`,n._svgName||n.fontIcon)(`data-mat-icon-namespace`,n._svgNamespace||n.fontSet)(`fontIcon`,n._usingFontIcon()?n.fontIcon:null),TD(n.color?`mat-`+n.color:``),nh(`mat-icon-inline`,n.inline)(`mat-icon-no-color`,n.color!==`primary`&&n.color!==`accent`&&n.color!==`warn`))},inputs:{color:`color`,inline:[2,`inline`,`inline`,JF],svgIcon:`svgIcon`,fontSet:`fontSet`,fontIcon:`fontIcon`},exportAs:[`matIcon`],ngContentSelectors:tt$2,decls:1,vars:0,template:function(e,n){e&1&&(aD(),cD(0))},styles:[`mat-icon, mat-icon.mat-primary, mat-icon.mat-accent, mat-icon.mat-warn {
  color: var(--%NS%mat-icon-color, inherit);
}

.mat-icon {
  -webkit-user-select: none;
  user-select: none;
  background-repeat: no-repeat;
  display: inline-block;
  fill: currentColor;
  height: 24px;
  width: 24px;
  overflow: hidden;
}
.mat-icon.mat-icon-inline {
  font-size: inherit;
  height: inherit;
  line-height: inherit;
  width: inherit;
}
.mat-icon.mat-ligature-font[fontIcon]::before {
  content: attr(fontIcon);
}

[dir=rtl] .mat-icon-rtl-mirror {
  transform: scale(-1, 1);
}

.mat-form-field:not(.mat-form-field-appearance-legacy) .mat-form-field-prefix .mat-icon,
.mat-form-field:not(.mat-form-field-appearance-legacy) .mat-form-field-suffix .mat-icon {
  display: block;
}
.mat-form-field:not(.mat-form-field-appearance-legacy) .mat-form-field-prefix .mat-icon-button .mat-icon,
.mat-form-field:not(.mat-form-field-appearance-legacy) .mat-form-field-suffix .mat-icon-button .mat-icon {
  margin: auto;
}
`],encapsulation:2})}return i})();var yt=(()=>{class i{static ɵfac=function(e){return new(e||i)};static ɵmod=IE({type:i});static ɵinj=Jl({imports:[yt$1]})}return i})();function U$1(i){return i&&typeof i.connect==`function`&&!(i instanceof ts)}var u=(function(i){return i[i.REPLACED=0]=`REPLACED`,i[i.INSERTED=1]=`INSERTED`,i[i.MOVED=2]=`MOVED`,i[i.REMOVED=3]=`REMOVED`,i})(u||{});var L=class{viewCacheSize=20;_viewCache=[];applyChanges(s,e,t,r,n){s.forEachOperation((o,d,f)=>{let _,p;if(o.previousIndex==null){let P=()=>t(o,d,f);_=this._insertView(P,f,e,r(o)),p=_?u.INSERTED:u.REPLACED}else f==null?(this._detachAndCacheView(d,e),p=u.REMOVED):(_=this._moveView(d,f,e,r(o)),p=u.MOVED);n&&n({context:_?.context,operation:p,record:o})})}detach(){for(let s of this._viewCache)s.destroy();this._viewCache=[]}_insertView(s,e,t,r){let n=this._insertViewFromCache(e,t);if(n){n.context.$implicit=r;return}let o=s();return t.createEmbeddedView(o.templateRef,o.context,o.index)}_detachAndCacheView(s,e){let t=e.detach(s);this._maybeCacheView(t,e)}_moveView(s,e,t,r){let n=t.get(s);return t.move(n,e),n.context.$implicit=r,n}_maybeCacheView(s,e){if(this._viewCache.length<this.viewCacheSize)this._viewCache.push(s);else{let t=e.indexOf(s);t===-1?s.destroy():e.remove(t)}}_insertViewFromCache(s,e){let t=this._viewCache.pop();return t&&e.insert(t,s),t||null}};var A$1=20;var N=(()=>{class i{_ngZone=T$1(xe);_platform=T$1(p$1);_renderer=T$1(gr).createRenderer(null,null);_cleanupGlobalListener;_scrolled=new z$1;_scrolledCount=0;scrollContainers=new Map;register(e){this.scrollContainers.has(e)||this.scrollContainers.set(e,e.elementScrolled().subscribe(()=>this._scrolled.next(e)))}deregister(e){let t=this.scrollContainers.get(e);t&&(t.unsubscribe(),this.scrollContainers.delete(e))}scrolled(e=A$1){return this._platform.isBrowser?new _(t=>{this._cleanupGlobalListener||(this._cleanupGlobalListener=this._ngZone.runOutsideAngular(()=>this._renderer.listen(`document`,`scroll`,()=>this._scrolled.next())));let r=e>0?this._scrolled.pipe(yg(e)).subscribe(t):this._scrolled.subscribe(t);return this._scrolledCount++,()=>{r.unsubscribe(),this._scrolledCount--,this._scrolledCount||(this._cleanupGlobalListener?.(),this._cleanupGlobalListener=void 0)}}):ss()}ngOnDestroy(){this._cleanupGlobalListener?.(),this._cleanupGlobalListener=void 0,this.scrollContainers.forEach((e,t)=>this.deregister(t)),this._scrolled.complete()}ancestorScrolled(e,t){let r=this.getAncestorScrollContainers(e);return this.scrolled(t).pipe(Bn(n=>!n||r.indexOf(n)>-1))}getAncestorScrollContainers(e){let t=[];return this.scrollContainers.forEach((r,n)=>{this._targetContainsElement(n,e)&&t.push(n)}),t}_targetContainsElement(e,t){let r=D$2(t),n=e.getElementRef().nativeElement;do if(r==n)return!0;while(r=r.parentElement);return!1}static ɵfac=function(t){return new(t||i)};static ɵprov=Ir({token:i,factory:i.ɵfac})}return i})();var Ye$1=(()=>{class i{elementRef=T$1(Er);scrollDispatcher=T$1(N);ngZone=T$1(xe);dir=T$1(Zn$1,{optional:!0});_scrollElement=this.elementRef.nativeElement;_destroyed=new z$1;_renderer=T$1($a);_cleanupScroll;_elementScrolled=new z$1;ngOnInit(){this._cleanupScroll=this.ngZone.runOutsideAngular(()=>this._renderer.listen(this._scrollElement,`scroll`,e=>this._elementScrolled.next(e))),this.scrollDispatcher.register(this)}ngOnDestroy(){this._cleanupScroll?.(),this._elementScrolled.complete(),this.scrollDispatcher.deregister(this),this._destroyed.next(),this._destroyed.complete()}elementScrolled(){return this._elementScrolled}getElementRef(){return this.elementRef}scrollTo(e){let t=this.elementRef.nativeElement,r=this.dir&&this.dir.value==`rtl`;e.left??=r?e.end:e.start,e.right??=r?e.start:e.end,e.bottom!=null&&(e.top=t.scrollHeight-t.clientHeight-e.bottom),r&&Zo()!=Y$2.NORMAL?(e.left!=null&&(e.right=t.scrollWidth-t.clientWidth-e.left),Zo()==Y$2.INVERTED?e.left=e.right:Zo()==Y$2.NEGATED&&(e.left=e.right?-e.right:e.right)):e.right!=null&&(e.left=t.scrollWidth-t.clientWidth-e.right),this._applyScrollToOptions(e)}_applyScrollToOptions(e){let t=this.elementRef.nativeElement;Wo()?t.scrollTo(e):(e.top!=null&&(t.scrollTop=e.top),e.left!=null&&(t.scrollLeft=e.left))}measureScrollOffset(e){let t=`left`,r=`right`,n=this.elementRef.nativeElement;if(e==`top`)return n.scrollTop;if(e==`bottom`)return n.scrollHeight-n.clientHeight-n.scrollTop;let o=this.dir&&this.dir.value==`rtl`;return e==`start`?e=o?r:t:e==`end`&&(e=o?t:r),o&&Zo()==Y$2.INVERTED?e==t?n.scrollWidth-n.clientWidth-n.scrollLeft:n.scrollLeft:o&&Zo()==Y$2.NEGATED?e==t?n.scrollLeft+n.scrollWidth-n.clientWidth:-n.scrollLeft:e==t?n.scrollLeft:n.scrollWidth-n.clientWidth-n.scrollLeft}static ɵfac=function(t){return new(t||i)};static ɵdir=wE({type:i,selectors:[[``,`cdk-scrollable`,``],[``,`cdkScrollable`,``]]})}return i})();var W$1=20;var Qe$1=(()=>{class i{_platform=T$1(p$1);_listeners;_viewportSize=null;_change=new z$1;_document=T$1(or);constructor(){let e=T$1(xe),t=T$1(gr).createRenderer(null,null);e.runOutsideAngular(()=>{if(this._platform.isBrowser){let r=n=>this._change.next(n);this._listeners=[t.listen(`window`,`resize`,r),t.listen(`window`,`orientationchange`,r)]}this.change().subscribe(()=>this._viewportSize=null)})}ngOnDestroy(){this._listeners?.forEach(e=>e()),this._change.complete()}getViewportSize(){this._viewportSize||this._updateViewportSize();let e={width:this._viewportSize.width,height:this._viewportSize.height};return this._platform.isBrowser||(this._viewportSize=null),e}getViewportRect(){let e=this.getViewportScrollPosition(),{width:t,height:r}=this.getViewportSize();return{top:e.top,left:e.left,bottom:e.top+r,right:e.left+t,height:r,width:t}}getViewportScrollPosition(){if(!this._platform.isBrowser)return{top:0,left:0};let e=this._document,t=this._getWindow(),r=e.documentElement,n=r.getBoundingClientRect();return{top:-n.top||e.body?.scrollTop||t.scrollY||r.scrollTop||0,left:-n.left||e.body?.scrollLeft||t.scrollX||r.scrollLeft||0}}change(e=W$1){return e>0?this._change.pipe(yg(e)):this._change}_getWindow(){return this._document.defaultView||window}_updateViewportSize(){let e=this._getWindow();this._viewportSize=this._platform.isBrowser?{width:e.innerWidth,height:e.innerHeight}:{width:0,height:0}}static ɵfac=function(t){return new(t||i)};static ɵprov=Ir({token:i,factory:i.ɵfac})}return i})();var Ke$1=new A$2(`CDK_VIRTUAL_SCROLL_VIEWPORT`);var I=(()=>{class i{static ɵfac=function(t){return new(t||i)};static ɵmod=IE({type:i});static ɵinj=Jl({})}return i})();var Xe$1=(()=>{class i{static ɵfac=function(t){return new(t||i)};static ɵmod=IE({type:i});static ɵinj=Jl({imports:[yt$1,I,yt$1,I]})}return i})();var D=class{_attachedHost=null;attach(t){return this._attachedHost=t,t.attach(this)}detach(){let t=this._attachedHost;t!=null&&(this._attachedHost=null,t.detach())}get isAttached(){return this._attachedHost!=null}setAttachedHost(t){this._attachedHost=t}};var at$1=class extends D{component;viewContainerRef;injector;projectableNodes;bindings;directives;constructor(t,e,i,s,n,r){super(),this.component=t,this.viewContainerRef=e,this.injector=i,this.projectableNodes=s,this.bindings=n||null,this.directives=r||null}};var M=class extends D{templateRef;viewContainerRef;context;injector;constructor(t,e,i,s){super(),this.templateRef=t,this.viewContainerRef=e,this.context=i,this.injector=s}get origin(){return this.templateRef.elementRef}attach(t,e=this.context){return this.context=e,super.attach(t)}detach(){return this.context=void 0,super.detach()}};var lt$1=class extends D{element;constructor(t){super(),this.element=t instanceof Er?t.nativeElement:t}};var j$1=class{_attachedPortal=null;_disposeFn=null;_isDisposed=!1;hasAttached(){return!!this._attachedPortal}attach(t){if(t instanceof at$1)return this._attachedPortal=t,this.attachComponentPortal(t);if(t instanceof M)return this._attachedPortal=t,this.attachTemplatePortal(t);if(this.attachDomPortal&&t instanceof lt$1)return this._attachedPortal=t,this.attachDomPortal(t)}attachDomPortal=null;detach(){this._attachedPortal&&(this._attachedPortal.setAttachedHost(null),this._attachedPortal=null),this._invokeDisposeFn()}dispose(){this.hasAttached()&&this.detach(),this._invokeDisposeFn(),this._isDisposed=!0}setDisposeFn(t){this._disposeFn=t}_invokeDisposeFn(){this._disposeFn&&(this._disposeFn(),this._disposeFn=null)}};var z=class extends j$1{outletElement;_appRef;_defaultInjector;constructor(t,e,i){super(),this.outletElement=t,this._appRef=e,this._defaultInjector=i}attachComponentPortal(t){let e;if(t.viewContainerRef){let i=t.injector||t.viewContainerRef.injector,s=i.get(Cn,null,{optional:!0})||void 0;e=t.viewContainerRef.createComponent(t.component,{index:t.viewContainerRef.length,injector:i,ngModuleRef:s,projectableNodes:t.projectableNodes||void 0,bindings:t.bindings||void 0,directives:t.directives||void 0}),this.setDisposeFn(()=>e.destroy())}else{let i=this._appRef,s=t.injector||this._defaultInjector||ge.NULL,n=s.get(le$1,i.injector);e=t1(t.component,{elementInjector:s,environmentInjector:n,projectableNodes:t.projectableNodes||void 0,bindings:t.bindings||void 0,directives:t.directives||void 0}),i.attachView(e.hostView),this.setDisposeFn(()=>{i.viewCount>0&&i.detachView(e.hostView),e.destroy()})}return this.outletElement.appendChild(this._getComponentRootNode(e)),this._attachedPortal=t,e}attachTemplatePortal(t){let e=t.viewContainerRef,i=e.createEmbeddedView(t.templateRef,t.context,{injector:t.injector});return i.rootNodes.forEach(s=>this.outletElement.appendChild(s)),i.detectChanges(),this.setDisposeFn(()=>{let s=e.indexOf(i);s!==-1&&e.remove(s)}),this._attachedPortal=t,i}attachDomPortal=t=>{let e=t.element;e.parentNode;let i=this.outletElement.ownerDocument.createComment(`dom-portal`);e.parentNode.insertBefore(i,e),this.outletElement.appendChild(e),this._attachedPortal=t,super.setDisposeFn(()=>{i.parentNode&&i.parentNode.replaceChild(e,i)})};dispose(){super.dispose(),this.outletElement.remove()}_getComponentRootNode(t){return t.hostView.rootNodes[0]}};var pe$1=(()=>{class o extends j$1{_moduleRef=T$1(Cn,{optional:!0});_document=T$1(or);_viewContainerRef=T$1(ki$1);_isInitialized=!1;_attachedRef=null;get portal(){return this._attachedPortal}set portal(e){this.hasAttached()&&!e&&!this._isInitialized||(this.hasAttached()&&super.detach(),e&&super.attach(e),this._attachedPortal=e||null)}attached=new Ue;get attachedRef(){return this._attachedRef}ngOnInit(){this._isInitialized=!0}ngOnDestroy(){super.dispose(),this._attachedRef=this._attachedPortal=null}attachComponentPortal(e){e.setAttachedHost(this);let i=e.viewContainerRef!=null?e.viewContainerRef:this._viewContainerRef,s=i.createComponent(e.component,{index:i.length,injector:e.injector||i.injector,projectableNodes:e.projectableNodes||void 0,ngModuleRef:this._moduleRef||void 0,bindings:e.bindings||void 0,directives:e.directives||void 0});return i!==this._viewContainerRef&&this._getRootNode().appendChild(s.hostView.rootNodes[0]),super.setDisposeFn(()=>s.destroy()),this._attachedPortal=e,this._attachedRef=s,this.attached.emit(s),s}attachTemplatePortal(e){e.setAttachedHost(this);let i=this._viewContainerRef.createEmbeddedView(e.templateRef,e.context,{injector:e.injector});return super.setDisposeFn(()=>this._viewContainerRef.clear()),this._attachedPortal=e,this._attachedRef=i,this.attached.emit(i),i}attachDomPortal=e=>{let i=e.element;i.parentNode;let s=this._document.createComment(`dom-portal`);e.setAttachedHost(this),i.parentNode.insertBefore(s,i),this._getRootNode().appendChild(i),this._attachedPortal=e,super.setDisposeFn(()=>{s.parentNode&&s.parentNode.replaceChild(i,s)})};_getRootNode(){let e=this._viewContainerRef.element.nativeElement;return e.nodeType===e.ELEMENT_NODE?e:e.parentNode}static ɵfac=(()=>{let e;return function(s){return(e||(e=ey(o)))(s||o)}})();static ɵdir=wE({type:o,selectors:[[``,`cdkPortalOutlet`,``]],inputs:{portal:[0,`cdkPortalOutlet`,`portal`]},outputs:{attached:`attached`},exportAs:[`cdkPortalOutlet`],features:[Rp]})}return o})();var Vt=(()=>{class o{static ɵfac=function(i){return new(i||o)};static ɵmod=IE({type:o});static ɵinj=Jl({})}return o})();var Nt$1=Wo();function Ht(o){return new Z(o.get(Qe$1),o.get(or))}var Z=class{_viewportRuler;_previousHTMLStyles={top:``,left:``};_previousScrollPosition;_isEnabled=!1;_document;constructor(t,e){this._viewportRuler=t,this._document=e}attach(){}enable(){if(this._canBeEnabled()){let t=this._document.documentElement;this._previousScrollPosition=this._viewportRuler.getViewportScrollPosition(),this._previousHTMLStyles.left=t.style.left||``,this._previousHTMLStyles.top=t.style.top||``,t.style.left=si$1(-this._previousScrollPosition.left),t.style.top=si$1(-this._previousScrollPosition.top),t.classList.add(`cdk-global-scrollblock`),this._isEnabled=!0}}disable(){if(this._isEnabled){let t=this._document.documentElement,e=this._document.body,i=t.style,s=e.style,n=i.scrollBehavior||``,r=s.scrollBehavior||``;this._isEnabled=!1,i.left=this._previousHTMLStyles.left,i.top=this._previousHTMLStyles.top,t.classList.remove(`cdk-global-scrollblock`),Nt$1&&(i.scrollBehavior=s.scrollBehavior=`auto`),window.scroll(this._previousScrollPosition.left,this._previousScrollPosition.top),Nt$1&&(i.scrollBehavior=n,s.scrollBehavior=r)}}_canBeEnabled(){if(this._document.documentElement.classList.contains(`cdk-global-scrollblock`)||this._isEnabled)return!1;let e=this._document.documentElement,i=this._viewportRuler.getViewportSize();return e.scrollHeight>i.height||e.scrollWidth>i.width}};function Wt(o,t){return new U(o.get(N),o.get(xe),o.get(Qe$1),t)}var U=class{_scrollDispatcher;_ngZone;_viewportRuler;_config;_scrollSubscription=null;_overlayRef;_initialScrollPosition;constructor(t,e,i,s){this._scrollDispatcher=t,this._ngZone=e,this._viewportRuler=i,this._config=s}attach(t){this._overlayRef,this._overlayRef=t}enable(){if(this._scrollSubscription)return;let t=this._scrollDispatcher.scrolled(0).pipe(Bn(e=>!e||!this._overlayRef.overlayElement.contains(e.getElementRef().nativeElement)));this._config&&this._config.threshold&&this._config.threshold>1?(this._initialScrollPosition=this._viewportRuler.getViewportScrollPosition().top,this._scrollSubscription=t.subscribe(()=>{let e=this._viewportRuler.getViewportScrollPosition().top;Math.abs(e-this._initialScrollPosition)>this._config.threshold?this._detach():this._overlayRef.updatePosition()})):this._scrollSubscription=t.subscribe(this._detach)}disable(){this._scrollSubscription&&(this._scrollSubscription.unsubscribe(),this._scrollSubscription=null)}detach(){this.disable(),this._overlayRef=null}_detach=()=>{this.disable(),this._overlayRef.hasAttached()&&this._ngZone.run(()=>this._overlayRef.detach())}};var A=class{enable(){}disable(){}attach(){}};function ht$1(o,t){return t.some(e=>{let i=o.bottom<e.top,s=o.top>e.bottom,n=o.right<e.left,r=o.left>e.right;return i||s||n||r})}function Ft$1(o,t){return t.some(e=>{let i=o.top<e.top,s=o.bottom>e.bottom,n=o.left<e.left,r=o.right>e.right;return i||s||n||r})}function pt$2(o,t){return new G$1(o.get(N),o.get(Qe$1),o.get(xe),t)}var G$1=class{_scrollDispatcher;_viewportRuler;_ngZone;_config;_scrollSubscription=null;_overlayRef;constructor(t,e,i,s){this._scrollDispatcher=t,this._viewportRuler=e,this._ngZone=i,this._config=s}attach(t){this._overlayRef,this._overlayRef=t}enable(){if(!this._scrollSubscription){let t=this._config?this._config.scrollThrottle:0;this._scrollSubscription=this._scrollDispatcher.scrolled(t).subscribe(()=>{if(this._overlayRef.updatePosition(),this._config&&this._config.autoClose){let e=this._overlayRef.overlayElement.getBoundingClientRect(),{width:i,height:s}=this._viewportRuler.getViewportSize();ht$1(e,[{width:i,height:s,bottom:s,right:i,top:0,left:0}])&&(this.disable(),this._ngZone.run(()=>this._overlayRef.detach()))}})}}disable(){this._scrollSubscription&&(this._scrollSubscription.unsubscribe(),this._scrollSubscription=null)}detach(){this.disable(),this._overlayRef=null}};var jt$1=(()=>{class o{_injector=T$1(ge);noop=()=>new A;close=e=>Wt(this._injector,e);block=()=>Ht(this._injector);reposition=e=>pt$2(this._injector,e);static ɵfac=function(i){return new(i||o)};static ɵprov=Ir({token:o,factory:o.ɵfac})}return o})();var B=class{positionStrategy;scrollStrategy=new A;panelClass=``;hasBackdrop=!1;backdropClass=`cdk-overlay-dark-backdrop`;disableAnimations;width;height;minWidth;minHeight;maxWidth;maxHeight;direction;disposeOnNavigation=!1;usePopover;eventPredicate;constructor(t){if(t){let e=Object.keys(t);for(let i of e)t[i]!==void 0&&(this[i]=t[i])}}};var K=class{connectionPair;scrollableViewProperties;constructor(t,e){this.connectionPair=t,this.scrollableViewProperties=e}};var zt$1=(()=>{class o{_attachedOverlays=[];_document=T$1(or);_isAttached=!1;ngOnDestroy(){this.detach()}add(e){this.remove(e),this._attachedOverlays.push(e)}remove(e){let i=this._attachedOverlays.indexOf(e);i>-1&&this._attachedOverlays.splice(i,1),this._attachedOverlays.length===0&&this.detach()}canReceiveEvent(e,i,s){return s.observers.length<1?!1:e.eventPredicate?e.eventPredicate(i):!0}static ɵfac=function(i){return new(i||o)};static ɵprov=Ir({token:o,factory:o.ɵfac})}return o})();var Zt=(()=>{class o extends zt$1{_ngZone=T$1(xe);_renderer=T$1(gr).createRenderer(null,null);_cleanupKeydown;add(e){super.add(e),this._isAttached||(this._ngZone.runOutsideAngular(()=>{this._cleanupKeydown=this._renderer.listen(`body`,`keydown`,this._keydownListener)}),this._isAttached=!0)}detach(){this._isAttached&&(this._cleanupKeydown?.(),this._isAttached=!1)}_keydownListener=e=>{let i=this._attachedOverlays;for(let s=i.length-1;s>-1;s--){let n=i[s];if(this.canReceiveEvent(n,e,n._keydownEvents)){this._ngZone.run(()=>n._keydownEvents.next(e));break}}};static ɵfac=function(i){return new(i||o)};static ɵprov=Ir({token:o,factory:o.ɵfac})}return o})();var Ut=(()=>{class o extends zt$1{_platform=T$1(p$1);_ngZone=T$1(xe);_renderer=T$1(gr).createRenderer(null,null);_cursorOriginalValue;_cursorStyleIsSet=!1;_pointerDownEventTarget=null;_cleanups;add(e){if(super.add(e),!this._isAttached){let i=this._document.body,s={capture:!0},n=this._renderer;this._cleanups=this._ngZone.runOutsideAngular(()=>[n.listen(i,`pointerdown`,this._pointerDownListener,s),n.listen(i,`click`,this._clickListener,s),n.listen(i,`auxclick`,this._clickListener,s),n.listen(i,`contextmenu`,this._clickListener,s)]),this._platform.IOS&&!this._cursorStyleIsSet&&(this._cursorOriginalValue=i.style.cursor,i.style.cursor=`pointer`,this._cursorStyleIsSet=!0),this._isAttached=!0}}detach(){this._isAttached&&(this._cleanups?.forEach(e=>e()),this._cleanups=void 0,this._platform.IOS&&this._cursorStyleIsSet&&(this._document.body.style.cursor=this._cursorOriginalValue,this._cursorStyleIsSet=!1),this._isAttached=!1)}_pointerDownListener=e=>{this._pointerDownEventTarget=y$1(e)};_clickListener=e=>{let i=y$1(e),s=e.type===`click`&&this._pointerDownEventTarget?this._pointerDownEventTarget:i;this._pointerDownEventTarget=null;let n=this._attachedOverlays.slice();for(let r=n.length-1;r>-1;r--){let a=n[r],h=a._outsidePointerEvents;if(!(!a.hasAttached()||!this.canReceiveEvent(a,e,h))){if(Yt(a.overlayElement,i)||Yt(a.overlayElement,s))break;this._ngZone?this._ngZone.run(()=>h.next(e)):h.next(e)}}};static ɵfac=function(i){return new(i||o)};static ɵprov=Ir({token:o,factory:o.ɵfac})}return o})();function Yt(o,t){let e=typeof ShadowRoot<`u`&&ShadowRoot,i=t;for(;i;){if(i===o)return!0;i=e&&i instanceof ShadowRoot?i.host:i.parentNode}return!1}var Gt=(()=>{class o{static ɵfac=function(i){return new(i||o)};static ɵcmp=yE({type:o,selectors:[[`ng-component`]],hostAttrs:[`cdk-overlay-style-loader`,``],decls:0,vars:0,template:function(i,s){},styles:[`.cdk-overlay-container, .cdk-global-overlay-wrapper {
  pointer-events: none;
  top: 0;
  left: 0;
  height: 100%;
  width: 100%;
}

.cdk-overlay-container {
  position: fixed;
}
@layer cdk-overlay {
  .cdk-overlay-container {
    z-index: 1000;
  }
}
.cdk-overlay-container:empty {
  display: none;
}

.cdk-global-overlay-wrapper {
  display: flex;
  position: absolute;
}
@layer cdk-overlay {
  .cdk-global-overlay-wrapper {
    z-index: 1000;
  }
}

.cdk-overlay-pane {
  position: absolute;
  pointer-events: auto;
  box-sizing: border-box;
  display: flex;
  max-width: 100%;
  max-height: 100%;
}
@layer cdk-overlay {
  .cdk-overlay-pane {
    z-index: 1000;
  }
}

.cdk-overlay-backdrop {
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
  pointer-events: auto;
  -webkit-tap-highlight-color: transparent;
  opacity: 0;
  touch-action: manipulation;
}
@layer cdk-overlay {
  .cdk-overlay-backdrop {
    z-index: 1000;
    transition: opacity 400ms cubic-bezier(0.25, 0.8, 0.25, 1);
  }
}
@media (prefers-reduced-motion) {
  .cdk-overlay-backdrop {
    transition-duration: 1ms;
  }
}

.cdk-overlay-backdrop-showing {
  opacity: 1;
}
@media (forced-colors: active) {
  .cdk-overlay-backdrop-showing {
    opacity: 0.6;
  }
}

@layer cdk-overlay {
  .cdk-overlay-dark-backdrop {
    background: rgba(0, 0, 0, 0.32);
  }
}

.cdk-overlay-transparent-backdrop {
  transition: visibility 1ms linear, opacity 1ms linear;
  visibility: hidden;
  opacity: 1;
}
.cdk-overlay-transparent-backdrop.cdk-overlay-backdrop-showing, .cdk-high-contrast-active .cdk-overlay-transparent-backdrop {
  opacity: 0;
  visibility: visible;
}

.cdk-overlay-backdrop-noop-animation {
  transition: none;
}

.cdk-overlay-connected-position-bounding-box {
  position: absolute;
  display: flex;
  flex-direction: column;
  min-width: 1px;
  min-height: 1px;
}
@layer cdk-overlay {
  .cdk-overlay-connected-position-bounding-box {
    z-index: 1000;
  }
}

.cdk-global-scrollblock {
  position: fixed;
  width: 100%;
  overflow-y: scroll;
}

.cdk-overlay-popover {
  background: none;
  border: none;
  padding: 0;
  outline: 0;
  overflow: visible;
  position: fixed;
  pointer-events: none;
  white-space: normal;
  color: inherit;
  text-decoration: none;
  width: 100%;
  height: 100%;
  inset: auto;
  top: 0;
  left: 0;
}
.cdk-overlay-popover::backdrop {
  display: none;
}
.cdk-overlay-popover .cdk-overlay-backdrop {
  position: fixed;
  z-index: auto;
}
`],encapsulation:2})}return o})();var Kt=(()=>{class o{_platform=T$1(p$1);_containerElement;_document=T$1(or);_styleLoader=T$1(A$3);ngOnDestroy(){this._containerElement?.remove()}getContainerElement(){return this._loadStyles(),this._containerElement||this._createContainer(),this._containerElement}_createContainer(){let e=`cdk-overlay-container`;if(this._platform.isBrowser||$o$1()){let s=this._document.querySelectorAll(`.${e}[platform="server"], .${e}[platform="test"]`);for(let n=0;n<s.length;n++)s[n].remove()}let i=this._document.createElement(`div`);i.classList.add(e),$o$1()?i.setAttribute(`platform`,`test`):this._platform.isBrowser||i.setAttribute(`platform`,`server`),this._document.body.appendChild(i),this._containerElement=i}_loadStyles(){this._styleLoader.load(Gt)}static ɵfac=function(i){return new(i||o)};static ɵprov=Ir({token:o,factory:o.ɵfac})}return o})();var ct$1=class{_renderer;_ngZone;element;_cleanupClick;_cleanupTransitionEnd;_fallbackTimeout;constructor(t,e,i,s){this._renderer=e,this._ngZone=i,this.element=t.createElement(`div`),this.element.classList.add(`cdk-overlay-backdrop`),this._cleanupClick=e.listen(this.element,`click`,s)}detach(){this._ngZone.runOutsideAngular(()=>{let t=this.element;clearTimeout(this._fallbackTimeout),this._cleanupTransitionEnd?.(),this._cleanupTransitionEnd=this._renderer.listen(t,`transitionend`,this.dispose),this._fallbackTimeout=setTimeout(this.dispose,500),t.style.pointerEvents=`none`,t.classList.remove(`cdk-overlay-backdrop-showing`)})}dispose=()=>{clearTimeout(this._fallbackTimeout),this._cleanupClick?.(),this._cleanupTransitionEnd?.(),this._cleanupClick=this._cleanupTransitionEnd=this._fallbackTimeout=void 0,this.element.remove()}};function ft$2(o){return o&&o.nodeType===1}var $$1=class{_portalOutlet;_host;_pane;_config;_ngZone;_keyboardDispatcher;_document;_location;_outsideClickDispatcher;_animationsDisabled;_injector;_renderer;_backdropClick=new z$1;_attachments=new z$1;_detachments=new z$1;_positionStrategy;_scrollStrategy;_locationChanges=j$2.EMPTY;_backdropRef=null;_detachContentMutationObserver;_detachContentAfterRenderRef;_disposed=!1;_previousHostParent;_keydownEvents=new z$1;_outsidePointerEvents=new z$1;_afterNextRenderRef;constructor(t,e,i,s,n,r,a,h,p,l=!1,d,u){this._portalOutlet=t,this._host=e,this._pane=i,this._config=s,this._ngZone=n,this._keyboardDispatcher=r,this._document=a,this._location=h,this._outsideClickDispatcher=p,this._animationsDisabled=l,this._injector=d,this._renderer=u,s.scrollStrategy&&(this._scrollStrategy=s.scrollStrategy,this._scrollStrategy.attach(this)),this._positionStrategy=s.positionStrategy}get overlayElement(){return this._pane}get backdropElement(){return this._backdropRef?.element||null}get hostElement(){return this._host}get eventPredicate(){return this._config?.eventPredicate||null}attach(t){if(this._disposed)return null;this._attachHost();let e=this._portalOutlet.attach(t);return this._positionStrategy?.attach(this),this._updateStackingOrder(),this._updateElementSize(),this._updateElementDirection(),this._scrollStrategy&&this._scrollStrategy.enable(),this._afterNextRenderRef?.destroy(),this._afterNextRenderRef=mv(()=>{this.hasAttached()&&this.updatePosition()},{injector:this._injector}),this._togglePointerEvents(!0),this._config.hasBackdrop&&this._attachBackdrop(),this._config.panelClass&&this._toggleClasses(this._pane,this._config.panelClass,!0),this._attachments.next(),this._completeDetachContent(),this._keyboardDispatcher.add(this),this._config.disposeOnNavigation&&(this._locationChanges=this._location.subscribe(()=>this.dispose())),this._outsideClickDispatcher.add(this),typeof e?.onDestroy==`function`&&e.onDestroy(()=>{this.hasAttached()&&this._ngZone.runOutsideAngular(()=>Promise.resolve().then(()=>this.detach()))}),e}detach(){if(!this.hasAttached())return;this.detachBackdrop(),this._togglePointerEvents(!1),this._positionStrategy&&this._positionStrategy.detach&&this._positionStrategy.detach(),this._scrollStrategy&&this._scrollStrategy.disable();let t=this._portalOutlet.detach();return this._detachments.next(),this._completeDetachContent(),this._keyboardDispatcher.remove(this),this._detachContentWhenEmpty(),this._locationChanges.unsubscribe(),this._outsideClickDispatcher.remove(this),t}dispose(){if(this._disposed)return;let t=this.hasAttached();this._positionStrategy&&this._positionStrategy.dispose(),this._disposeScrollStrategy(),this._backdropRef?.dispose(),this._locationChanges.unsubscribe(),this._keyboardDispatcher.remove(this),this._portalOutlet.dispose(),this._attachments.complete(),this._backdropClick.complete(),this._keydownEvents.complete(),this._outsidePointerEvents.complete(),this._outsideClickDispatcher.remove(this),this._host?.remove(),this._afterNextRenderRef?.destroy(),this._previousHostParent=this._pane=this._host=this._backdropRef=null,t&&this._detachments.next(),this._detachments.complete(),this._completeDetachContent(),this._disposed=!0}hasAttached(){return this._portalOutlet.hasAttached()}backdropClick(){return this._backdropClick}attachments(){return this._attachments}detachments(){return this._detachments}keydownEvents(){return this._keydownEvents}outsidePointerEvents(){return this._outsidePointerEvents}getConfig(){return this._config}updatePosition(){this._positionStrategy&&this._positionStrategy.apply()}updatePositionStrategy(t){t!==this._positionStrategy&&(this._positionStrategy&&this._positionStrategy.dispose(),this._positionStrategy=t,this.hasAttached()&&(t.attach(this),this.updatePosition()))}updateSize(t){this._config=l(l({},this._config),t),this._updateElementSize()}setDirection(t){this._config=m(l({},this._config),{direction:t}),this._updateElementDirection()}addPanelClass(t){this._pane&&this._toggleClasses(this._pane,t,!0)}removePanelClass(t){this._pane&&this._toggleClasses(this._pane,t,!1)}getDirection(){let t=this._config.direction;return t?typeof t==`string`?t:t.value:`ltr`}updateScrollStrategy(t){t!==this._scrollStrategy&&(this._disposeScrollStrategy(),this._scrollStrategy=t,this.hasAttached()&&(t.attach(this),t.enable()))}_updateElementDirection(){this._host.setAttribute(`dir`,this.getDirection())}_updateElementSize(){if(!this._pane)return;let t=this._pane.style;t.width=si$1(this._config.width),t.height=si$1(this._config.height),t.minWidth=si$1(this._config.minWidth),t.minHeight=si$1(this._config.minHeight),t.maxWidth=si$1(this._config.maxWidth),t.maxHeight=si$1(this._config.maxHeight)}_togglePointerEvents(t){this._pane.style.pointerEvents=t?``:`none`}_attachHost(){if(!this._host.parentElement){let t=this._config.usePopover?this._positionStrategy?.getPopoverInsertionPoint?.():null;ft$2(t)?t.after(this._host):t?.type===`parent`?t.element.appendChild(this._host):this._previousHostParent?.appendChild(this._host)}if(this._config.usePopover)try{this._host.showPopover()}catch{}}_attachBackdrop(){let t=`cdk-overlay-backdrop-showing`;this._backdropRef?.dispose(),this._backdropRef=new ct$1(this._document,this._renderer,this._ngZone,e=>{this._backdropClick.next(e)}),this._animationsDisabled&&this._backdropRef.element.classList.add(`cdk-overlay-backdrop-noop-animation`),this._config.backdropClass&&this._toggleClasses(this._backdropRef.element,this._config.backdropClass,!0),this._config.usePopover?this._host.prepend(this._backdropRef.element):this._host.parentElement.insertBefore(this._backdropRef.element,this._host),!this._animationsDisabled&&typeof requestAnimationFrame<`u`?this._ngZone.runOutsideAngular(()=>{requestAnimationFrame(()=>this._backdropRef?.element.classList.add(t))}):this._backdropRef.element.classList.add(t)}_updateStackingOrder(){!this._config.usePopover&&this._host.nextSibling&&this._host.parentNode.appendChild(this._host)}detachBackdrop(){this._animationsDisabled?(this._backdropRef?.dispose(),this._backdropRef=null):this._backdropRef?.detach()}_toggleClasses(t,e,i){let s=Ct$1(e||[]).filter(n=>!!n);s.length&&(i?t.classList.add(...s):t.classList.remove(...s))}_detachContentWhenEmpty(){let t=!1;try{this._detachContentAfterRenderRef=mv(()=>{t=!0,this._detachContent()},{injector:this._injector})}catch(e){if(t)throw e;this._detachContent()}globalThis.MutationObserver&&this._pane&&(this._detachContentMutationObserver||=new globalThis.MutationObserver(()=>{this._detachContent()}),this._detachContentMutationObserver.observe(this._pane,{childList:!0}))}_detachContent(){(!this._pane||!this._host||this._pane.children.length===0)&&(this._pane&&this._config.panelClass&&this._toggleClasses(this._pane,this._config.panelClass,!1),this._host&&this._host.parentElement&&(this._previousHostParent=this._host.parentElement,this._host.remove()),this._completeDetachContent())}_completeDetachContent(){this._detachContentAfterRenderRef?.destroy(),this._detachContentAfterRenderRef=void 0,this._detachContentMutationObserver?.disconnect()}_disposeScrollStrategy(){let t=this._scrollStrategy;t?.disable(),t?.detach?.()}};var Tt=`cdk-overlay-connected-position-bounding-box`;var ee$1=/([A-Za-z%]+)$/;function ut$2(o,t){return new q(t,o.get(Qe$1),o.get(or),o.get(p$1),o.get(Kt))}var q=class{_viewportRuler;_document;_platform;_overlayContainer;_overlayRef;_isInitialRender=!1;_lastBoundingBoxSize={width:0,height:0};_isPushed=!1;_canPush=!0;_growAfterOpen=!1;_hasFlexibleDimensions=!0;_positionLocked=!1;_originRect;_overlayRect;_viewportRect;_containerRect;_viewportMargin=0;_scrollables=[];_preferredPositions=[];_origin;_pane;_isDisposed=!1;_boundingBox=null;_lastPosition=null;_lastScrollVisibility=null;_positionChanges=new z$1;_resizeSubscription=j$2.EMPTY;_offsetX=0;_offsetY=0;_transformOriginSelector;_appliedPanelClasses=[];_previousPushAmount=null;_popoverLocation=`global`;positionChanges=this._positionChanges;get positions(){return this._preferredPositions}constructor(t,e,i,s,n){this._viewportRuler=e,this._document=i,this._platform=s,this._overlayContainer=n,this.setOrigin(t)}attach(t){this._overlayRef&&this._overlayRef,this._validatePositions(),t.hostElement.classList.add(Tt),this._overlayRef=t,this._boundingBox=t.hostElement,this._pane=t.overlayElement,this._isDisposed=!1,this._isInitialRender=!0,this._lastPosition=null,this._resizeSubscription.unsubscribe(),this._resizeSubscription=this._viewportRuler.change().subscribe(()=>{this._isInitialRender=!0,this.apply()})}apply(){if(this._isDisposed||!this._platform.isBrowser)return;if(!this._isInitialRender&&this._positionLocked&&this._lastPosition){this.reapplyLastPosition();return}this._clearPanelClasses(),this._resetOverlayElementStyles(),this._resetBoundingBoxStyles(),this._viewportRect=this._getNarrowedViewportRect(),this._originRect=this._getOriginRect(),this._overlayRect=this._pane.getBoundingClientRect(),this._containerRect=this._getContainerRect();let t=this._originRect,e=this._overlayRect,i=this._viewportRect,s=this._containerRect,n=[],r;for(let a of this._preferredPositions){let h=this._getOriginPoint(t,s,a),p=this._getOverlayPoint(h,e,a),l=this._getOverlayFit(p,e,i,a);if(l.isCompletelyWithinViewport){this._isPushed=!1,this._applyPosition(a,h);return}if(this._canFitWithFlexibleDimensions(l,p,i)){n.push({position:a,origin:h,overlayRect:e,boundingBoxRect:this._calculateBoundingBoxRect(h,a)});continue}(!r||r.overlayFit.visibleArea<l.visibleArea)&&(r={overlayFit:l,overlayPoint:p,originPoint:h,position:a,overlayRect:e})}if(n.length){let a=null,h=-1;for(let p of n){let l=p.boundingBoxRect.width*p.boundingBoxRect.height*(p.position.weight||1);l>h&&(h=l,a=p)}this._isPushed=!1,this._applyPosition(a.position,a.origin);return}if(this._canPush){this._isPushed=!0,this._applyPosition(r.position,r.originPoint);return}this._applyPosition(r.position,r.originPoint)}detach(){this._clearPanelClasses(),this._lastPosition=null,this._previousPushAmount=null,this._resizeSubscription.unsubscribe()}dispose(){this._isDisposed||(this._boundingBox&&R(this._boundingBox.style,{top:``,left:``,right:``,bottom:``,height:``,width:``,alignItems:``,justifyContent:``}),this._pane&&this._resetOverlayElementStyles(),this._overlayRef&&this._overlayRef.hostElement.classList.remove(Tt),this.detach(),this._positionChanges.complete(),this._overlayRef=this._boundingBox=null,this._isDisposed=!0)}reapplyLastPosition(){if(this._isDisposed||!this._platform.isBrowser)return;let t=this._lastPosition;t?(this._originRect=this._getOriginRect(),this._overlayRect=this._pane.getBoundingClientRect(),this._viewportRect=this._getNarrowedViewportRect(),this._containerRect=this._getContainerRect(),this._applyPosition(t,this._getOriginPoint(this._originRect,this._containerRect,t))):this.apply()}withScrollableContainers(t){return this._scrollables=t,this}withPositions(t){return this._preferredPositions=t,t.indexOf(this._lastPosition)===-1&&(this._lastPosition=null),this._validatePositions(),this}withViewportMargin(t){return this._viewportMargin=t,this}withFlexibleDimensions(t=!0){return this._hasFlexibleDimensions=t,this}withGrowAfterOpen(t=!0){return this._growAfterOpen=t,this}withPush(t=!0){return this._canPush=t,this}withLockedPosition(t=!0){return this._positionLocked=t,this}setOrigin(t){return this._origin=t,this}withDefaultOffsetX(t){return this._offsetX=t,this}withDefaultOffsetY(t){return this._offsetY=t,this}withTransformOriginOn(t){return this._transformOriginSelector=t,this}withPopoverLocation(t){return this._popoverLocation=t,this}getPopoverInsertionPoint(){return this._popoverLocation===`global`?null:this._popoverLocation!==`inline`?this._popoverLocation:this._origin instanceof Er?this._origin.nativeElement:ft$2(this._origin)?this._origin:null}_getOriginPoint(t,e,i){let s;if(i.originX==`center`)s=t.left+t.width/2;else{let r=this._isRtl()?t.right:t.left,a=this._isRtl()?t.left:t.right;s=i.originX==`start`?r:a}e.left<0&&(s-=e.left);let n;return i.originY==`center`?n=t.top+t.height/2:n=i.originY==`top`?t.top:t.bottom,e.top<0&&(n-=e.top),{x:s,y:n}}_getOverlayPoint(t,e,i){let s;i.overlayX==`center`?s=-e.width/2:i.overlayX===`start`?s=this._isRtl()?-e.width:0:s=this._isRtl()?0:-e.width;let n;return i.overlayY==`center`?n=-e.height/2:n=i.overlayY==`top`?0:-e.height,{x:t.x+s,y:t.y+n}}_getOverlayFit(t,e,i,s){let n=Xt(e),{x:r,y:a}=t,h=this._getOffset(s,`x`),p=this._getOffset(s,`y`);h&&(r+=h),p&&(a+=p);let l=0-r,d=r+n.width-i.width,u=0-a,g=a+n.height-i.height,_=this._subtractOverflows(n.width,l,d),v=this._subtractOverflows(n.height,u,g),vt=_*v;return{visibleArea:vt,isCompletelyWithinViewport:n.width*n.height===vt,fitsInViewportVertically:v===n.height,fitsInViewportHorizontally:_==n.width}}_canFitWithFlexibleDimensions(t,e,i){if(this._hasFlexibleDimensions){let s=i.bottom-e.y,n=i.right-e.x,r=Lt$1(this._overlayRef.getConfig().minHeight),a=Lt$1(this._overlayRef.getConfig().minWidth),h=t.fitsInViewportVertically||r!=null&&r<=s,p=t.fitsInViewportHorizontally||a!=null&&a<=n;return h&&p}return!1}_pushOverlayOnScreen(t,e,i){if(this._previousPushAmount&&this._positionLocked)return{x:t.x+this._previousPushAmount.x,y:t.y+this._previousPushAmount.y};let s=Xt(e),n=this._viewportRect,r=Math.max(t.x+s.width-n.width,0),a=Math.max(t.y+s.height-n.height,0),h=Math.max(n.top-i.top-t.y,0),p=Math.max(n.left-i.left-t.x,0),l=0,d=0;return s.width<=n.width?l=p||-r:l=t.x<this._getViewportMarginStart()?n.left-i.left-t.x:0,s.height<=n.height?d=h||-a:d=t.y<this._getViewportMarginTop()?n.top-i.top-t.y:0,this._previousPushAmount={x:l,y:d},{x:t.x+l,y:t.y+d}}_applyPosition(t,e){if(this._setTransformOrigin(t),this._setOverlayElementStyles(e,t),this._setBoundingBoxStyles(e,t),t.panelClass&&this._addPanelClasses(t.panelClass),this._positionChanges.observers.length){let i=this._getScrollVisibility();if(t!==this._lastPosition||!this._lastScrollVisibility||!ie$1(this._lastScrollVisibility,i)){let s=new K(t,i);this._positionChanges.next(s)}this._lastScrollVisibility=i}this._lastPosition=t,this._isInitialRender=!1}_setTransformOrigin(t){if(!this._transformOriginSelector)return;let e=this._boundingBox.querySelectorAll(this._transformOriginSelector),i,s=t.overlayY;t.overlayX===`center`?i=`center`:this._isRtl()?i=t.overlayX===`start`?`right`:`left`:i=t.overlayX===`start`?`left`:`right`;for(let n=0;n<e.length;n++)e[n].style.transformOrigin=`${i} ${s}`}_calculateBoundingBoxRect(t,e){let i=this._viewportRect,s=this._isRtl(),n,r,a;if(e.overlayY===`top`)r=t.y,n=i.height-r+this._getViewportMarginBottom();else if(e.overlayY===`bottom`)a=i.height-t.y+this._getViewportMarginTop()+this._getViewportMarginBottom(),n=i.height-a+this._getViewportMarginTop();else{let g=Math.min(i.bottom-t.y+i.top,t.y),_=this._lastBoundingBoxSize.height;n=g*2,r=t.y-g,n>_&&!this._isInitialRender&&!this._growAfterOpen&&(r=t.y-_/2)}let h=e.overlayX===`start`&&!s||e.overlayX===`end`&&s,p=e.overlayX===`end`&&!s||e.overlayX===`start`&&s,l,d,u;if(p)u=i.width-t.x+this._getViewportMarginStart()+this._getViewportMarginEnd(),l=t.x-this._getViewportMarginStart();else if(h)d=t.x,l=i.right-t.x-this._getViewportMarginEnd();else{let g=Math.min(i.right-t.x+i.left,t.x),_=this._lastBoundingBoxSize.width;l=g*2,d=t.x-g,l>_&&!this._isInitialRender&&!this._growAfterOpen&&(d=t.x-_/2)}return{top:r,left:d,bottom:a,right:u,width:l,height:n}}_setBoundingBoxStyles(t,e){let i=this._calculateBoundingBoxRect(t,e);!this._isInitialRender&&!this._growAfterOpen&&(i.height=Math.min(i.height,this._lastBoundingBoxSize.height),i.width=Math.min(i.width,this._lastBoundingBoxSize.width));let s={};if(this._hasExactPosition())s.top=s.left=`0`,s.bottom=s.right=`auto`,s.maxHeight=s.maxWidth=``,s.width=s.height=`100%`;else{let n=this._overlayRef.getConfig().maxHeight,r=this._overlayRef.getConfig().maxWidth;s.width=si$1(i.width),s.height=si$1(i.height),s.top=si$1(i.top)||`auto`,s.bottom=si$1(i.bottom)||`auto`,s.left=si$1(i.left)||`auto`,s.right=si$1(i.right)||`auto`,e.overlayX===`center`?s.alignItems=`center`:s.alignItems=e.overlayX===`end`?`flex-end`:`flex-start`,e.overlayY===`center`?s.justifyContent=`center`:s.justifyContent=e.overlayY===`bottom`?`flex-end`:`flex-start`,n&&(s.maxHeight=si$1(n)),r&&(s.maxWidth=si$1(r))}this._lastBoundingBoxSize=i,R(this._boundingBox.style,s)}_resetBoundingBoxStyles(){R(this._boundingBox.style,{top:`0`,left:`0`,right:`0`,bottom:`0`,height:``,width:``,alignItems:``,justifyContent:``})}_resetOverlayElementStyles(){R(this._pane.style,{top:``,left:``,bottom:``,right:``,position:``,transform:``})}_setOverlayElementStyles(t,e){let i={},s=this._hasExactPosition(),n=this._hasFlexibleDimensions,r=this._overlayRef.getConfig();if(s){let l=this._viewportRuler.getViewportScrollPosition();R(i,this._getExactOverlayY(e,t,l)),R(i,this._getExactOverlayX(e,t,l))}else i.position=`static`;let a=``,h=this._getOffset(e,`x`),p=this._getOffset(e,`y`);h&&(a+=`translateX(${h}px) `),p&&(a+=`translateY(${p}px)`),i.transform=a.trim(),r.maxHeight&&(s?i.maxHeight=si$1(r.maxHeight):n&&(i.maxHeight=``)),r.maxWidth&&(s?i.maxWidth=si$1(r.maxWidth):n&&(i.maxWidth=``)),R(this._pane.style,i)}_getExactOverlayY(t,e,i){let s={top:``,bottom:``},n=this._getOverlayPoint(e,this._overlayRect,t);if(this._isPushed&&(n=this._pushOverlayOnScreen(n,this._overlayRect,i)),t.overlayY===`bottom`)s.bottom=`${this._document.documentElement.clientHeight-(n.y+this._overlayRect.height)}px`;else s.top=si$1(n.y);return s}_getExactOverlayX(t,e,i){let s={left:``,right:``},n=this._getOverlayPoint(e,this._overlayRect,t);this._isPushed&&(n=this._pushOverlayOnScreen(n,this._overlayRect,i));let r;if(this._isRtl()?r=t.overlayX===`end`?`left`:`right`:r=t.overlayX===`end`?`right`:`left`,r===`right`)s.right=`${this._document.documentElement.clientWidth-(n.x+this._overlayRect.width)}px`;else s.left=si$1(n.x);return s}_getScrollVisibility(){let t=this._getOriginRect(),e=this._pane.getBoundingClientRect(),i=this._scrollables.map(s=>s.getElementRef().nativeElement.getBoundingClientRect());return{isOriginClipped:Ft$1(t,i),isOriginOutsideView:ht$1(t,i),isOverlayClipped:Ft$1(e,i),isOverlayOutsideView:ht$1(e,i)}}_subtractOverflows(t,...e){return e.reduce((i,s)=>i-Math.max(s,0),t)}_getNarrowedViewportRect(){let t=this._document.documentElement.clientWidth,e=this._document.documentElement.clientHeight,i=this._viewportRuler.getViewportScrollPosition();return{top:i.top+this._getViewportMarginTop(),left:i.left+this._getViewportMarginStart(),right:i.left+t-this._getViewportMarginEnd(),bottom:i.top+e-this._getViewportMarginBottom(),width:t-this._getViewportMarginStart()-this._getViewportMarginEnd(),height:e-this._getViewportMarginTop()-this._getViewportMarginBottom()}}_isRtl(){return this._overlayRef.getDirection()===`rtl`}_hasExactPosition(){return!this._hasFlexibleDimensions||this._isPushed}_getOffset(t,e){return e===`x`?t.offsetX==null?this._offsetX:t.offsetX:t.offsetY==null?this._offsetY:t.offsetY}_validatePositions(){}_addPanelClasses(t){this._pane&&Ct$1(t).forEach(e=>{e!==``&&this._appliedPanelClasses.indexOf(e)===-1&&(this._appliedPanelClasses.push(e),this._pane.classList.add(e))})}_clearPanelClasses(){this._pane&&(this._appliedPanelClasses.forEach(t=>{this._pane.classList.remove(t)}),this._appliedPanelClasses=[])}_getViewportMarginStart(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.start??0}_getViewportMarginEnd(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.end??0}_getViewportMarginTop(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.top??0}_getViewportMarginBottom(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.bottom??0}_getOriginRect(){let t=this._origin;if(t instanceof Er)return t.nativeElement.getBoundingClientRect();if(t instanceof Element)return t.getBoundingClientRect();let e=t.width||0,i=t.height||0;return{top:t.y,bottom:t.y+i,left:t.x,right:t.x+e,height:i,width:e}}_getContainerRect(){let t=this._overlayRef.getConfig().usePopover&&this._popoverLocation!==`global`,e=this._overlayContainer.getContainerElement();t&&(e.style.display=`block`);let i=e.getBoundingClientRect();return t&&(e.style.display=``),i}};function R(o,t){for(let e in t)t.hasOwnProperty(e)&&(o[e]=t[e]);return o}function Lt$1(o){if(typeof o!=`number`&&o!=null){let[t,e]=o.split(ee$1);return!e||e===`px`?parseFloat(t):null}return o||null}function Xt(o){return{top:Math.floor(o.top),right:Math.floor(o.right),bottom:Math.floor(o.bottom),left:Math.floor(o.left),width:Math.floor(o.width),height:Math.floor(o.height)}}function ie$1(o,t){return o===t?!0:o.isOriginClipped===t.isOriginClipped&&o.isOriginOutsideView===t.isOriginOutsideView&&o.isOverlayClipped===t.isOverlayClipped&&o.isOverlayOutsideView===t.isOverlayOutsideView}var It=`cdk-global-overlay-wrapper`;function $t(o){return new J$1}var J$1=class{_overlayRef;_cssPosition=`static`;_topOffset=``;_bottomOffset=``;_alignItems=``;_xPosition=``;_xOffset=``;_width=``;_height=``;_isDisposed=!1;attach(t){let e=t.getConfig();this._overlayRef=t,this._width&&!e.width&&t.updateSize({width:this._width}),this._height&&!e.height&&t.updateSize({height:this._height}),t.hostElement.classList.add(It),this._isDisposed=!1}top(t=``){return this._bottomOffset=``,this._topOffset=t,this._alignItems=`flex-start`,this}left(t=``){return this._xOffset=t,this._xPosition=`left`,this}bottom(t=``){return this._topOffset=``,this._bottomOffset=t,this._alignItems=`flex-end`,this}right(t=``){return this._xOffset=t,this._xPosition=`right`,this}start(t=``){return this._xOffset=t,this._xPosition=`start`,this}end(t=``){return this._xOffset=t,this._xPosition=`end`,this}width(t=``){return this._overlayRef?this._overlayRef.updateSize({width:t}):this._width=t,this}height(t=``){return this._overlayRef?this._overlayRef.updateSize({height:t}):this._height=t,this}centerHorizontally(t=``){return this.left(t),this._xPosition=`center`,this}centerVertically(t=``){return this.top(t),this._alignItems=`center`,this}apply(){if(!this._overlayRef||!this._overlayRef.hasAttached())return;let t=this._overlayRef.overlayElement.style,e=this._overlayRef.hostElement.style,{width:s,height:n,maxWidth:r,maxHeight:a}=this._overlayRef.getConfig(),h=(s===`100%`||s===`100vw`)&&(!r||r===`100%`||r===`100vw`),p=(n===`100%`||n===`100vh`)&&(!a||a===`100%`||a===`100vh`),l=this._xPosition,d=this._xOffset,u=this._overlayRef.getConfig().direction===`rtl`,g=``,_=``,v=``;h?v=`flex-start`:l===`center`?(v=`center`,u?_=d:g=d):u?l===`left`||l===`end`?(v=`flex-end`,g=d):(l===`right`||l===`start`)&&(v=`flex-start`,_=d):l===`left`||l===`start`?(v=`flex-start`,g=d):(l===`right`||l===`end`)&&(v=`flex-end`,_=d),t.position=this._cssPosition,t.marginLeft=h?`0`:g,t.marginTop=p?`0`:this._topOffset,t.marginBottom=this._bottomOffset,t.marginRight=h?`0`:_,e.justifyContent=v,e.alignItems=p?`flex-start`:this._alignItems}dispose(){if(this._isDisposed||!this._overlayRef)return;let t=this._overlayRef.overlayElement.style,e=this._overlayRef.hostElement,i=e.style;e.classList.remove(It),i.justifyContent=i.alignItems=t.marginTop=t.marginBottom=t.marginLeft=t.marginRight=t.position=``,this._overlayRef=null,this._isDisposed=!0}};var qt=(()=>{class o{_injector=T$1(ge);global(){return $t()}flexibleConnectedTo(e){return ut$2(this._injector,e)}static ɵfac=function(i){return new(i||o)};static ɵprov=Ir({token:o,factory:o.ɵfac})}return o})();var _t$1=new A$2(`OVERLAY_DEFAULT_CONFIG`);function gt$1(o,t){o.get(A$3).load(Gt);let e=o.get(Kt),i=o.get(or),s=o.get(Pt$1),n=o.get(Oi$1),r=o.get(Zn$1),a=o.get($a,null,{optional:!0})||o.get(gr).createRenderer(null,null),h=new B(t),p=o.get(_t$1,null,{optional:!0})?.usePopover??!0;h.direction=h.direction||r.value,!i.body||!(`showPopover`in i.body)?h.usePopover=!1:h.usePopover=t?.usePopover??p;let l=i.createElement(`div`),d=i.createElement(`div`);l.id=s.getId(`cdk-overlay-`),l.classList.add(`cdk-overlay-pane`),d.appendChild(l),h.usePopover&&(d.setAttribute(`popover`,`manual`),d.classList.add(`cdk-overlay-popover`));let u=h.usePopover?h.positionStrategy?.getPopoverInsertionPoint?.():null;return ft$2(u)?u.after(d):u?.type===`parent`?u.element.appendChild(d):e.getContainerElement().appendChild(d),new $$1(new z(l,n,o),d,l,h,o.get(xe),o.get(Zt),i,o.get(Ye$2),o.get(Ut),t?.disableAnimations??o.get(vm,null,{optional:!0})===`NoopAnimations`,o.get(le$1),a)}var Jt$1=(()=>{class o{scrollStrategies=T$1(jt$1);_positionBuilder=T$1(qt);_injector=T$1(ge);create(e){return gt$1(this._injector,e)}position(){return this._positionBuilder}static ɵfac=function(i){return new(i||o)};static ɵprov=Ir({token:o,factory:o.ɵfac})}return o})();var oe$1=[{originX:`start`,originY:`bottom`,overlayX:`start`,overlayY:`top`},{originX:`start`,originY:`top`,overlayX:`start`,overlayY:`bottom`},{originX:`end`,originY:`top`,overlayX:`end`,overlayY:`bottom`},{originX:`end`,originY:`bottom`,overlayX:`end`,overlayY:`top`}];var se$1=new A$2(`cdk-connected-overlay-scroll-strategy`,{providedIn:`root`,factory:()=>{let o=T$1(ge);return()=>pt$2(o)}});var dt$1=(()=>{class o{elementRef=T$1(Er);static ɵfac=function(i){return new(i||o)};static ɵdir=wE({type:o,selectors:[[``,`cdk-overlay-origin`,``],[``,`overlay-origin`,``],[``,`cdkOverlayOrigin`,``]],exportAs:[`cdkOverlayOrigin`]})}return o})();var Qt=new A$2(`cdk-connected-overlay-default-config`);var ne$1=(()=>{class o{_dir=T$1(Zn$1,{optional:!0});_injector=T$1(ge);_overlayRef;_templatePortal;_backdropSubscription=j$2.EMPTY;_attachSubscription=j$2.EMPTY;_detachSubscription=j$2.EMPTY;_positionSubscription=j$2.EMPTY;_offsetX;_offsetY;_position;_scrollStrategyFactory=T$1(se$1);_ngZone=T$1(xe);origin;positions;positionStrategy;get offsetX(){return this._offsetX}set offsetX(e){this._offsetX=e,this._position&&this._updatePositionStrategy(this._position)}get offsetY(){return this._offsetY}set offsetY(e){this._offsetY=e,this._position&&this._updatePositionStrategy(this._position)}width;height;minWidth;minHeight;backdropClass;panelClass;viewportMargin=0;scrollStrategy;open=!1;disableClose=!1;transformOriginSelector;hasBackdrop=!1;lockPosition=!1;flexibleDimensions=!1;growAfterOpen=!1;push=!1;disposeOnNavigation=!1;usePopover;matchWidth=!1;set _config(e){typeof e!=`string`&&this._assignConfig(e)}backdropClick=new Ue;positionChange=new Ue;attach=new Ue;detach=new Ue;overlayKeydown=new Ue;overlayOutsideClick=new Ue;constructor(){let e=T$1(hr),i=T$1(ki$1),s=T$1(Qt,{optional:!0}),n=T$1(_t$1,{optional:!0});this.usePopover=n?.usePopover===!1?null:`global`,this._templatePortal=new M(e,i),this.scrollStrategy=this._scrollStrategyFactory(),s&&this._assignConfig(s)}get overlayRef(){return this._overlayRef}get dir(){return this._dir?this._dir.value:`ltr`}ngOnDestroy(){this._attachSubscription.unsubscribe(),this._detachSubscription.unsubscribe(),this._backdropSubscription.unsubscribe(),this._positionSubscription.unsubscribe(),this._overlayRef?.dispose()}ngOnChanges(e){this._position&&(this._updatePositionStrategy(this._position),this._overlayRef?.updateSize({width:this._getWidth(),minWidth:this.minWidth,height:this.height,minHeight:this.minHeight}),e.origin&&this.open&&this._position.apply()),e.open&&(this.open?this.attachOverlay():this.detachOverlay())}_createOverlay(){(!this.positions||!this.positions.length)&&(this.positions=oe$1);let e=this._overlayRef=gt$1(this._injector,this._buildConfig());this._attachSubscription=e.attachments().subscribe(()=>this.attach.emit()),this._detachSubscription=e.detachments().subscribe(()=>this.detach.emit()),e.keydownEvents().subscribe(i=>{this.overlayKeydown.next(i),i.keyCode===27&&!this.disableClose&&!je$1(i)&&(i.preventDefault(),this.detachOverlay())}),this._overlayRef.outsidePointerEvents().subscribe(i=>{let s=this._getOriginElement(),n=y$1(i);(!s||s!==n&&!s.contains(n))&&this.overlayOutsideClick.next(i)})}_buildConfig(){let e=this._position=this.positionStrategy||this._createPositionStrategy(),i=new B({direction:this._dir||`ltr`,positionStrategy:e,scrollStrategy:this.scrollStrategy,hasBackdrop:this.hasBackdrop,disposeOnNavigation:this.disposeOnNavigation,usePopover:!!this.usePopover});return(this.height||this.height===0)&&(i.height=this.height),(this.minWidth||this.minWidth===0)&&(i.minWidth=this.minWidth),(this.minHeight||this.minHeight===0)&&(i.minHeight=this.minHeight),this.backdropClass&&(i.backdropClass=this.backdropClass),this.panelClass&&(i.panelClass=this.panelClass),i}_updatePositionStrategy(e){let i=this.positions.map(s=>({originX:s.originX,originY:s.originY,overlayX:s.overlayX,overlayY:s.overlayY,offsetX:s.offsetX||this.offsetX,offsetY:s.offsetY||this.offsetY,panelClass:s.panelClass||void 0}));return e.setOrigin(this._getOrigin()).withPositions(i).withFlexibleDimensions(this.flexibleDimensions).withPush(this.push).withGrowAfterOpen(this.growAfterOpen).withViewportMargin(this.viewportMargin).withLockedPosition(this.lockPosition).withTransformOriginOn(this.transformOriginSelector).withPopoverLocation(this.usePopover===null?`global`:this.usePopover)}_createPositionStrategy(){let e=ut$2(this._injector,this._getOrigin());return this._updatePositionStrategy(e),e}_getOrigin(){return this.origin instanceof dt$1?this.origin.elementRef:this.origin}_getOriginElement(){return this.origin instanceof dt$1?this.origin.elementRef.nativeElement:this.origin instanceof Er?this.origin.nativeElement:typeof Element<`u`&&this.origin instanceof Element?this.origin:null}_getWidth(){return this.width?this.width:this.matchWidth?this._getOriginElement()?.getBoundingClientRect?.().width:void 0}attachOverlay(){this._overlayRef||this._createOverlay();let e=this._overlayRef;e.getConfig().hasBackdrop=this.hasBackdrop,e.updateSize({width:this._getWidth()}),e.hasAttached()||e.attach(this._templatePortal),this.hasBackdrop?this._backdropSubscription=e.backdropClick().subscribe(i=>this.backdropClick.emit(i)):this._backdropSubscription.unsubscribe(),this._positionSubscription.unsubscribe(),this.positionChange.observers.length>0&&(this._positionSubscription=this._position.positionChanges.pipe(Lg(()=>this.positionChange.observers.length>0)).subscribe(i=>{this._ngZone.run(()=>this.positionChange.emit(i)),this.positionChange.observers.length===0&&this._positionSubscription.unsubscribe()})),this.open=!0}detachOverlay(){this._overlayRef?.detach(),this._backdropSubscription.unsubscribe(),this._positionSubscription.unsubscribe(),this.open=!1}_assignConfig(e){this.origin=e.origin??this.origin,this.positions=e.positions??this.positions,this.positionStrategy=e.positionStrategy??this.positionStrategy,this.offsetX=e.offsetX??this.offsetX,this.offsetY=e.offsetY??this.offsetY,this.width=e.width??this.width,this.height=e.height??this.height,this.minWidth=e.minWidth??this.minWidth,this.minHeight=e.minHeight??this.minHeight,this.backdropClass=e.backdropClass??this.backdropClass,this.panelClass=e.panelClass??this.panelClass,this.viewportMargin=e.viewportMargin??this.viewportMargin,this.scrollStrategy=e.scrollStrategy??this.scrollStrategy,this.disableClose=e.disableClose??this.disableClose,this.transformOriginSelector=e.transformOriginSelector??this.transformOriginSelector,this.hasBackdrop=e.hasBackdrop??this.hasBackdrop,this.lockPosition=e.lockPosition??this.lockPosition,this.flexibleDimensions=e.flexibleDimensions??this.flexibleDimensions,this.growAfterOpen=e.growAfterOpen??this.growAfterOpen,this.push=e.push??this.push,this.disposeOnNavigation=e.disposeOnNavigation??this.disposeOnNavigation,this.usePopover=e.usePopover??this.usePopover,this.matchWidth=e.matchWidth??this.matchWidth}static ɵfac=function(i){return new(i||o)};static ɵdir=wE({type:o,selectors:[[``,`cdk-connected-overlay`,``],[``,`connected-overlay`,``],[``,`cdkConnectedOverlay`,``]],inputs:{origin:[0,`cdkConnectedOverlayOrigin`,`origin`],positions:[0,`cdkConnectedOverlayPositions`,`positions`],positionStrategy:[0,`cdkConnectedOverlayPositionStrategy`,`positionStrategy`],offsetX:[0,`cdkConnectedOverlayOffsetX`,`offsetX`],offsetY:[0,`cdkConnectedOverlayOffsetY`,`offsetY`],width:[0,`cdkConnectedOverlayWidth`,`width`],height:[0,`cdkConnectedOverlayHeight`,`height`],minWidth:[0,`cdkConnectedOverlayMinWidth`,`minWidth`],minHeight:[0,`cdkConnectedOverlayMinHeight`,`minHeight`],backdropClass:[0,`cdkConnectedOverlayBackdropClass`,`backdropClass`],panelClass:[0,`cdkConnectedOverlayPanelClass`,`panelClass`],viewportMargin:[0,`cdkConnectedOverlayViewportMargin`,`viewportMargin`],scrollStrategy:[0,`cdkConnectedOverlayScrollStrategy`,`scrollStrategy`],open:[0,`cdkConnectedOverlayOpen`,`open`],disableClose:[0,`cdkConnectedOverlayDisableClose`,`disableClose`],transformOriginSelector:[0,`cdkConnectedOverlayTransformOriginOn`,`transformOriginSelector`],hasBackdrop:[2,`cdkConnectedOverlayHasBackdrop`,`hasBackdrop`,JF],lockPosition:[2,`cdkConnectedOverlayLockPosition`,`lockPosition`,JF],flexibleDimensions:[2,`cdkConnectedOverlayFlexibleDimensions`,`flexibleDimensions`,JF],growAfterOpen:[2,`cdkConnectedOverlayGrowAfterOpen`,`growAfterOpen`,JF],push:[2,`cdkConnectedOverlayPush`,`push`,JF],disposeOnNavigation:[2,`cdkConnectedOverlayDisposeOnNavigation`,`disposeOnNavigation`,JF],usePopover:[0,`cdkConnectedOverlayUsePopover`,`usePopover`],matchWidth:[2,`cdkConnectedOverlayMatchWidth`,`matchWidth`,JF],_config:[0,`cdkConnectedOverlay`,`_config`]},outputs:{backdropClick:`backdropClick`,positionChange:`positionChange`,attach:`attach`,detach:`detach`,overlayKeydown:`overlayKeydown`,overlayOutsideClick:`overlayOutsideClick`},exportAs:[`cdkConnectedOverlay`],features:[Lm]})}return o})();var re$1=(()=>{class o{static ɵfac=function(i){return new(i||o)};static ɵmod=IE({type:o});static ɵinj=Jl({providers:[Jt$1],imports:[yt$1,Vt,Xe$1,Xe$1]})}return o})();var j=class{_box;_destroyed=new z$1;_resizeSubject=new z$1;_resizeObserver;_elementObservables=new Map;constructor(r){this._box=r,typeof ResizeObserver<`u`&&(this._resizeObserver=new ResizeObserver(e=>this._resizeSubject.next(e)))}observe(r){return this._elementObservables.has(r)||this._elementObservables.set(r,new _(e=>{let n=this._resizeSubject.subscribe(e);return this._resizeObserver?.observe(r,{box:this._box}),()=>{this._resizeObserver?.unobserve(r),n.unsubscribe(),this._elementObservables.delete(r)}}).pipe(Bn(e=>e.some(n=>n.target===r)),Ul({bufferSize:1,refCount:!0}),Og(this._destroyed))),this._elementObservables.get(r)}destroy(){this._destroyed.next(),this._destroyed.complete(),this._resizeSubject.complete(),this._elementObservables.clear()}};var we=(()=>{class t{_cleanupErrorListener;_observers=new Map;_ngZone=T$1(xe);constructor(){}ngOnDestroy(){for(let[,e]of this._observers)e.destroy();this._observers.clear(),this._cleanupErrorListener?.()}observe(e,n){let i=n?.box||`content-box`;return this._observers.has(i)||this._observers.set(i,new j(i)),this._observers.get(i).observe(e)}static ɵfac=function(n){return new(n||t)};static ɵprov=Ir({token:t,factory:t.ɵfac})}return t})();var Ye=[`notch`];var Xe=[`*`];var Me=[`iconPrefixContainer`];var ze=[`textPrefixContainer`];var Le=[`iconSuffixContainer`];var Re=[`textSuffixContainer`];var Ke=[`textField`];var Je=[`*`,[[`mat-label`]],[[``,`matPrefix`,``],[``,`matIconPrefix`,``]],[[``,`matTextPrefix`,``]],[[``,`matTextSuffix`,``]],[[``,`matSuffix`,``],[``,`matIconSuffix`,``]],[[`mat-error`],[``,`matError`,``]],[[`mat-hint`,3,`align`,`end`]],[[`mat-hint`,`align`,`end`]]];var et=[`*`,`mat-label`,`[matPrefix], [matIconPrefix]`,`[matTextPrefix]`,`[matTextSuffix]`,`[matSuffix], [matIconSuffix]`,`mat-error, [matError]`,`mat-hint:not([align='end'])`,`mat-hint[align='end']`];function tt$1(t,r){t&1&&Bp(0,`span`,21)}function it(t,r){if(t&1&&(Ei$1(0,`label`,20),cD(1,1),$E(2,tt$1,1,0,`span`,21),Hc()),t&2){let e=iD(2);Hp(`floating`,e._shouldLabelFloat())(`monitorResize`,e._hasOutline())(`id`,e._labelId),Vp(`for`,e._control.disableAutomaticLabeling?null:e._control.id),Bv(2),UE(!e.hideRequiredMarker&&e._control.required?2:-1)}}function nt(t,r){if(t&1&&$E(0,it,3,5,`label`,20),t&2)UE(iD()._hasFloatingLabel()?0:-1)}function ot(t,r){t&1&&Bp(0,`div`,7)}function rt(t,r){}function lt(t,r){if(t&1&&Op(0,rt,0,0,`ng-template`,13),t&2){iD(2);Hp(`ngTemplateOutlet`,pD(1))}}function at(t,r){if(t&1&&(Ei$1(0,`div`,9),$E(1,lt,1,1,null,13),Hc()),t&2){let e=iD();Hp(`matFormFieldNotchedOutlineOpen`,e._shouldLabelFloat()),Bv(),UE(e._forceDisplayInfixLabel()?-1:1)}}function dt(t,r){t&1&&(Ei$1(0,`div`,10,2),cD(2,2),Hc())}function mt(t,r){t&1&&(Ei$1(0,`div`,11,3),cD(2,3),Hc())}function ct(t,r){}function ft$1(t,r){if(t&1&&Op(0,ct,0,0,`ng-template`,13),t&2){iD();Hp(`ngTemplateOutlet`,pD(1))}}function st(t,r){t&1&&(Ei$1(0,`div`,14,4),cD(2,4),Hc())}function ut$1(t,r){t&1&&(Ei$1(0,`div`,15,5),cD(2,5),Hc())}function pt$1(t,r){t&1&&Bp(0,`div`,16)}function ht(t,r){t&1&&(Ei$1(0,`div`,18),cD(1,6),Hc())}function bt(t,r){if(t&1&&(Ei$1(0,`mat-hint`,22),PD(1),Hc()),t&2){let e=iD(2);Hp(`id`,e._hintLabelId),Bv(),ah(e.hintLabel)}}function xt(t,r){if(t&1&&(Ei$1(0,`div`,19),$E(1,bt,2,2,`mat-hint`,22),cD(2,7),Bp(3,`div`,23),cD(4,8),Hc()),t&2){let e=iD();Bv(),UE(e.hintLabel?1:-1)}}var W=(()=>{class t{static ɵfac=function(n){return new(n||t)};static ɵdir=wE({type:t,selectors:[[`mat-label`]]})}return t})();var G=new A$2(`MatError`);var _t=(()=>{class t{id=T$1(Pt$1).getId(`mat-mdc-error-`);static ɵfac=function(n){return new(n||t)};static ɵdir=wE({type:t,selectors:[[`mat-error`],[``,`matError`,``]],hostAttrs:[1,`mat-mdc-form-field-error`,`mat-mdc-form-field-bottom-align`],hostVars:1,hostBindings:function(n,i){n&2&&Wp(`id`,i.id)},inputs:{id:`id`},features:[qD([{provide:G,useExisting:t}])]})}return t})();var V=(()=>{class t{align=`start`;id=T$1(Pt$1).getId(`mat-mdc-hint-`);static ɵfac=function(n){return new(n||t)};static ɵdir=wE({type:t,selectors:[[`mat-hint`]],hostAttrs:[1,`mat-mdc-form-field-hint`,`mat-mdc-form-field-bottom-align`],hostVars:4,hostBindings:function(n,i){n&2&&(Wp(`id`,i.id),Vp(`align`,null),nh(`mat-mdc-form-field-hint-end`,i.align===`end`))},inputs:{align:`align`,id:`id`}})}return t})();var Ae=new A$2(`MatPrefix`);var Ie=new A$2(`MatSuffix`);var Be=new A$2(`FloatingLabelParent`);var Ee=(()=>{class t{_elementRef=T$1(Er);get floating(){return this._floating}set floating(e){this._floating=e,this.monitorResize&&this._handleResize()}_floating=!1;get monitorResize(){return this._monitorResize}set monitorResize(e){this._monitorResize=e,this._monitorResize?this._subscribeToResize():this._resizeSubscription.unsubscribe()}_monitorResize=!1;_resizeObserver=T$1(we);_ngZone=T$1(xe);_parent=T$1(Be);_resizeSubscription=new j$2;ngOnDestroy(){this._resizeSubscription.unsubscribe()}getWidth(){return gt(this._elementRef.nativeElement)}get element(){return this._elementRef.nativeElement}_handleResize(){setTimeout(()=>this._parent._handleLabelResized())}_subscribeToResize(){this._resizeSubscription.unsubscribe(),this._ngZone.runOutsideAngular(()=>{this._resizeSubscription=this._resizeObserver.observe(this._elementRef.nativeElement,{box:`border-box`}).subscribe(()=>this._handleResize())})}static ɵfac=function(n){return new(n||t)};static ɵdir=wE({type:t,selectors:[[`label`,`matFormFieldFloatingLabel`,``]],hostAttrs:[1,`mdc-floating-label`,`mat-mdc-floating-label`],hostVars:2,hostBindings:function(n,i){n&2&&nh(`mdc-floating-label--float-above`,i.floating)},inputs:{floating:`floating`,monitorResize:`monitorResize`}})}return t})();function gt(t){let r=t;if(r.offsetParent!==null)return r.scrollWidth;let e=r.cloneNode(!0);e.style.setProperty(`position`,`absolute`),e.style.setProperty(`transform`,`translate(-9999px, -9999px)`),document.documentElement.appendChild(e);let n=e.scrollWidth;return e.remove(),n}var Te=`mdc-line-ripple--active`;var T=`mdc-line-ripple--deactivating`;var De=(()=>{class t{_elementRef=T$1(Er);_cleanupTransitionEnd;constructor(){let e=T$1(xe),n=T$1($a);e.runOutsideAngular(()=>{this._cleanupTransitionEnd=n.listen(this._elementRef.nativeElement,`transitionend`,this._handleTransitionEnd)})}activate(){let e=this._elementRef.nativeElement.classList;e.remove(T),e.add(Te)}deactivate(){this._elementRef.nativeElement.classList.add(T)}_handleTransitionEnd=e=>{let n=this._elementRef.nativeElement.classList,i=n.contains(T);e.propertyName===`opacity`&&i&&n.remove(Te,T)};ngOnDestroy(){this._cleanupTransitionEnd()}static ɵfac=function(n){return new(n||t)};static ɵdir=wE({type:t,selectors:[[`div`,`matFormFieldLineRipple`,``]],hostAttrs:[1,`mdc-line-ripple`]})}return t})();var Oe=(()=>{class t{_elementRef=T$1(Er);_ngZone=T$1(xe);open=!1;_notch;ngAfterViewInit(){let e=this._elementRef.nativeElement,n=e.querySelector(`.mdc-floating-label`);n?(e.classList.add(`mdc-notched-outline--upgraded`),typeof requestAnimationFrame==`function`&&(n.style.transitionDuration=`0s`,this._ngZone.runOutsideAngular(()=>{requestAnimationFrame(()=>n.style.transitionDuration=``)}))):e.classList.add(`mdc-notched-outline--no-label`)}_setNotchWidth(e){let n=this._notch.nativeElement;!this.open||!e?n.style.width=``:n.style.width=`calc(${e}px * var(--mat-mdc-form-field-floating-label-scale, 0.75) + 9px)`}_setMaxWidth(e){this._notch.nativeElement.style.setProperty(`--mat-form-field-notch-max-width`,`calc(100% - ${e}px)`)}static ɵfac=function(n){return new(n||t)};static ɵcmp=yE({type:t,selectors:[[`div`,`matFormFieldNotchedOutline`,``]],viewQuery:function(n,i){if(n&1&&Kp(Ye,5),n&2){let o;uD(o=dD())&&(i._notch=o.first)}},hostAttrs:[1,`mdc-notched-outline`],hostVars:2,hostBindings:function(n,i){n&2&&nh(`mdc-notched-outline--notched`,i.open)},inputs:{open:[0,`matFormFieldNotchedOutlineOpen`,`open`]},ngContentSelectors:Xe,decls:5,vars:0,consts:[[`notch`,``],[1,`mat-mdc-notch-piece`,`mdc-notched-outline__leading`],[1,`mat-mdc-notch-piece`,`mdc-notched-outline__notch`],[1,`mat-mdc-notch-piece`,`mdc-notched-outline__trailing`]],template:function(n,i){n&1&&(aD(),$p(0,`div`,1),Bc(1,`div`,2,0),cD(3),$c(),$p(4,`div`,3))},encapsulation:2})}return t})();var qe=(()=>{class t{value=null;stateChanges;id;placeholder;ngControl=null;focused=!1;empty=!1;shouldLabelFloat=!1;required=!1;disabled=!1;errorState=!1;controlType;autofilled;userAriaDescribedBy;disableAutomaticLabeling;describedByIds;static ɵfac=function(n){return new(n||t)};static ɵdir=wE({type:t})}return t})();var He=new A$2(`MatFormField`);var Qe=new A$2(`MAT_FORM_FIELD_DEFAULT_OPTIONS`);var ke=`fill`;var vt=`auto`;var Pe=`fixed`;var St=`translateY(-50%)`;var je=(()=>{class t{_elementRef=T$1(Er);_changeDetectorRef=T$1(YF);_platform=T$1(p$1);_idGenerator=T$1(Pt$1);_ngZone=T$1(xe);_defaults=T$1(Qe,{optional:!0});_currentDirection;_textField;_iconPrefixContainer;_textPrefixContainer;_iconSuffixContainer;_textSuffixContainer;_floatingLabel;_notchedOutline;_lineRipple;_iconPrefixContainerSignal=zF(`iconPrefixContainer`);_textPrefixContainerSignal=zF(`textPrefixContainer`);_iconSuffixContainerSignal=zF(`iconSuffixContainer`);_textSuffixContainerSignal=zF(`textSuffixContainer`);_prefixSuffixContainers=nw(()=>[this._iconPrefixContainerSignal(),this._textPrefixContainerSignal(),this._iconSuffixContainerSignal(),this._textSuffixContainerSignal()].map(e=>e?.nativeElement).filter(e=>e!==void 0));_formFieldControl;_prefixChildren;_suffixChildren;_errorChildren;_hintChildren;_labelChild=QF(W);get hideRequiredMarker(){return this._hideRequiredMarker}set hideRequiredMarker(e){this._hideRequiredMarker=di$1(e)}_hideRequiredMarker=!1;color=`primary`;get floatLabel(){return this._floatLabel||this._defaults?.floatLabel||vt}set floatLabel(e){e!==this._floatLabel&&(this._floatLabel=e,this._changeDetectorRef.markForCheck())}_floatLabel;get appearance(){return this._appearanceSignal()}set appearance(e){let n=e||this._defaults?.appearance||ke;this._appearanceSignal.set(n)}_appearanceSignal=$o(ke);get subscriptSizing(){return this._subscriptSizing||this._defaults?.subscriptSizing||Pe}set subscriptSizing(e){this._subscriptSizing=e||this._defaults?.subscriptSizing||Pe}_subscriptSizing=null;get hintLabel(){return this._hintLabel}set hintLabel(e){this._hintLabel=e,this._processHints()}_hintLabel=``;_hasIconPrefix=!1;_hasTextPrefix=!1;_hasIconSuffix=!1;_hasTextSuffix=!1;_labelId=this._idGenerator.getId(`mat-mdc-form-field-label-`);_hintLabelId=this._idGenerator.getId(`mat-mdc-hint-`);_describedByIds;get _control(){return this._explicitFormFieldControl||this._formFieldControl}set _control(e){this._explicitFormFieldControl=e}_destroyed=new z$1;_isFocused=null;_explicitFormFieldControl;_previousControl=null;_previousControlValidatorFn=null;_stateChanges;_valueChanges;_describedByChanges;_outlineLabelOffsetResizeObserver=null;_animationsDisabled=z$2();constructor(){let e=this._defaults,n=T$1(Zn$1);e&&(e.appearance&&(this.appearance=e.appearance),this._hideRequiredMarker=!!e?.hideRequiredMarker,e.color&&(this.color=e.color)),Zu(()=>this._currentDirection=n.valueSignal()),this._syncOutlineLabelOffset()}ngAfterViewInit(){this._updateFocusState(),this._animationsDisabled||this._ngZone.runOutsideAngular(()=>{setTimeout(()=>{this._elementRef.nativeElement.classList.add(`mat-form-field-animations-enabled`)},300)}),this._changeDetectorRef.detectChanges()}ngAfterContentInit(){this._assertFormFieldControl(),this._initializeSubscript(),this._initializePrefixAndSuffix()}ngAfterContentChecked(){this._assertFormFieldControl(),this._control!==this._previousControl&&(this._initializeControl(this._previousControl),this._control.ngControl&&this._control.ngControl.control&&(this._previousControlValidatorFn=this._control.ngControl.control.validator),this._previousControl=this._control),this._control.ngControl&&this._control.ngControl.control&&this._control.ngControl.control.validator!==this._previousControlValidatorFn&&this._changeDetectorRef.markForCheck()}ngOnDestroy(){this._outlineLabelOffsetResizeObserver?.disconnect(),this._stateChanges?.unsubscribe(),this._valueChanges?.unsubscribe(),this._describedByChanges?.unsubscribe(),this._destroyed.next(),this._destroyed.complete()}getLabelId=nw(()=>this._hasFloatingLabel()?this._labelId:null);getConnectedOverlayOrigin(){return this._textField||this._elementRef}_animateAndLockLabel(){this._hasFloatingLabel()&&(this.floatLabel=`always`)}_initializeControl(e){let n=this._control,i=`mat-mdc-form-field-type-`;e&&this._elementRef.nativeElement.classList.remove(i+e.controlType),n.controlType&&this._elementRef.nativeElement.classList.add(i+n.controlType),this._stateChanges?.unsubscribe(),this._stateChanges=n.stateChanges.subscribe(()=>{this._updateFocusState(),this._changeDetectorRef.markForCheck()}),this._describedByChanges?.unsubscribe(),this._describedByChanges=n.stateChanges.pipe(kg([void 0,void 0]),ce$1(()=>[n.errorState,n.userAriaDescribedBy]),Sg(),Bn(([[o,l],[b,D]])=>o!==b||l!==D)).subscribe(()=>this._syncDescribedByIds()),this._valueChanges?.unsubscribe(),n.ngControl&&n.ngControl.valueChanges&&(this._valueChanges=n.ngControl.valueChanges.pipe(Og(this._destroyed)).subscribe(()=>this._changeDetectorRef.markForCheck()))}_checkPrefixAndSuffixTypes(){this._hasIconPrefix=!!this._prefixChildren.find(e=>!e._isText),this._hasTextPrefix=!!this._prefixChildren.find(e=>e._isText),this._hasIconSuffix=!!this._suffixChildren.find(e=>!e._isText),this._hasTextSuffix=!!this._suffixChildren.find(e=>e._isText)}_initializePrefixAndSuffix(){this._checkPrefixAndSuffixTypes(),mg(this._prefixChildren.changes,this._suffixChildren.changes).subscribe(()=>{this._checkPrefixAndSuffixTypes(),this._changeDetectorRef.markForCheck()})}_initializeSubscript(){this._hintChildren.changes.subscribe(()=>{this._processHints(),this._changeDetectorRef.markForCheck()}),this._errorChildren.changes.subscribe(()=>{this._syncDescribedByIds(),this._changeDetectorRef.markForCheck()}),this._validateHints(),this._syncDescribedByIds()}_assertFormFieldControl(){this._control}_updateFocusState(){let e=this._control.focused;e&&!this._isFocused?(this._isFocused=!0,this._lineRipple?.activate()):!e&&(this._isFocused||this._isFocused===null)&&(this._isFocused=!1,this._lineRipple?.deactivate()),this._elementRef.nativeElement.classList.toggle(`mat-focused`,e),this._textField?.nativeElement.classList.toggle(`mdc-text-field--focused`,e)}_syncOutlineLabelOffset(){e1({earlyRead:()=>{if(this._appearanceSignal()!==`outline`)return this._outlineLabelOffsetResizeObserver?.disconnect(),null;if(globalThis.ResizeObserver){this._outlineLabelOffsetResizeObserver||=new globalThis.ResizeObserver(()=>{this._writeOutlinedLabelStyles(this._getOutlinedLabelOffset())});for(let e of this._prefixSuffixContainers())this._outlineLabelOffsetResizeObserver.observe(e,{box:`border-box`})}return this._getOutlinedLabelOffset()},write:e=>this._writeOutlinedLabelStyles(e())})}_shouldAlwaysFloat(){return this.floatLabel===`always`}_hasOutline(){return this.appearance===`outline`}_forceDisplayInfixLabel(){return!this._platform.isBrowser&&this._prefixChildren.length&&!this._shouldLabelFloat()}_hasFloatingLabel=nw(()=>!!this._labelChild());_shouldLabelFloat(){return this._hasFloatingLabel()?this._control.shouldLabelFloat||this._shouldAlwaysFloat():!1}_shouldForward(e){let n=this._control?this._control.ngControl:null;return n&&n[e]}_getSubscriptMessageType(){return this._errorChildren&&this._errorChildren.length>0&&this._control.errorState?`error`:`hint`}_handleLabelResized(){this._refreshOutlineNotchWidth()}_refreshOutlineNotchWidth(){!this._hasOutline()||!this._floatingLabel||!this._shouldLabelFloat()?this._notchedOutline?._setNotchWidth(0):this._notchedOutline?._setNotchWidth(this._floatingLabel.getWidth())}_processHints(){this._validateHints(),this._syncDescribedByIds()}_validateHints(){this._hintChildren}_syncDescribedByIds(){if(this._control){let e=[];if(this._control.userAriaDescribedBy&&typeof this._control.userAriaDescribedBy==`string`&&e.push(...this._control.userAriaDescribedBy.split(` `)),this._getSubscriptMessageType()===`hint`){let o=this._hintChildren?this._hintChildren.find(b=>b.align===`start`):null,l=this._hintChildren?this._hintChildren.find(b=>b.align===`end`):null;o?e.push(o.id):this._hintLabel&&e.push(this._hintLabelId),l&&e.push(l.id)}else this._errorChildren&&e.push(...this._errorChildren.map(o=>o.id));let n=this._control.describedByIds,i;if(n){let o=this._describedByIds||e;i=e.concat(n.filter(l=>l&&!o.includes(l)))}else i=e;this._control.setDescribedByIds(i),this._describedByIds=e}}_getOutlinedLabelOffset(){if(!this._hasOutline()||!this._floatingLabel)return null;if(!this._iconPrefixContainer&&!this._textPrefixContainer)return[``,null];if(!this._isAttachedToDom())return null;let e=this._iconPrefixContainer?.nativeElement,n=this._textPrefixContainer?.nativeElement,i=this._iconSuffixContainer?.nativeElement,o=this._textSuffixContainer?.nativeElement,l=e?.getBoundingClientRect().width??0,b=n?.getBoundingClientRect().width??0,D=i?.getBoundingClientRect().width??0,We=o?.getBoundingClientRect().width??0;return[`var(--mat-mdc-form-field-label-transform, ${St} translateX(${`calc(${this._currentDirection===`rtl`?`-1`:`1`} * (${`${l+b}px`} + var(--mat-mdc-form-field-label-offset-x, 0px)))`}))`,l+b+D+We]}_writeOutlinedLabelStyles(e){if(e!==null){let[n,i]=e;this._floatingLabel&&(this._floatingLabel.element.style.transform=n),i!==null&&this._notchedOutline?._setMaxWidth(i)}}_isAttachedToDom(){let e=this._elementRef.nativeElement;if(e.getRootNode){let n=e.getRootNode();return n&&n!==e}return document.documentElement.contains(e)}static ɵfac=function(n){return new(n||t)};static ɵcmp=yE({type:t,selectors:[[`mat-form-field`]],contentQueries:function(n,i,o){if(n&1&&(Jp(o,i._labelChild,W,5),Yp(o,qe,5)(o,Ae,5)(o,Ie,5)(o,G,5)(o,V,5)),n&2){fD();let l;uD(l=dD())&&(i._formFieldControl=l.first),uD(l=dD())&&(i._prefixChildren=l),uD(l=dD())&&(i._suffixChildren=l),uD(l=dD())&&(i._errorChildren=l),uD(l=dD())&&(i._hintChildren=l)}},viewQuery:function(n,i){if(n&1&&(Xp(i._iconPrefixContainerSignal,Me,5)(i._textPrefixContainerSignal,ze,5)(i._iconSuffixContainerSignal,Le,5)(i._textSuffixContainerSignal,Re,5),Kp(Ke,5)(Me,5)(ze,5)(Le,5)(Re,5)(Ee,5)(Oe,5)(De,5)),n&2){fD(4);let o;uD(o=dD())&&(i._textField=o.first),uD(o=dD())&&(i._iconPrefixContainer=o.first),uD(o=dD())&&(i._textPrefixContainer=o.first),uD(o=dD())&&(i._iconSuffixContainer=o.first),uD(o=dD())&&(i._textSuffixContainer=o.first),uD(o=dD())&&(i._floatingLabel=o.first),uD(o=dD())&&(i._notchedOutline=o.first),uD(o=dD())&&(i._lineRipple=o.first)}},hostAttrs:[1,`mat-mdc-form-field`],hostVars:38,hostBindings:function(n,i){n&2&&nh(`mat-mdc-form-field-label-always-float`,i._shouldAlwaysFloat())(`mat-mdc-form-field-has-icon-prefix`,i._hasIconPrefix)(`mat-mdc-form-field-has-icon-suffix`,i._hasIconSuffix)(`mat-form-field-invalid`,i._control.errorState)(`mat-form-field-disabled`,i._control.disabled)(`mat-form-field-autofilled`,i._control.autofilled)(`mat-form-field-appearance-fill`,i.appearance==`fill`)(`mat-form-field-appearance-outline`,i.appearance==`outline`)(`mat-form-field-hide-placeholder`,i._hasFloatingLabel()&&!i._shouldLabelFloat())(`mat-primary`,i.color!==`accent`&&i.color!==`warn`)(`mat-accent`,i.color===`accent`)(`mat-warn`,i.color===`warn`)(`ng-untouched`,i._shouldForward(`untouched`))(`ng-touched`,i._shouldForward(`touched`))(`ng-pristine`,i._shouldForward(`pristine`))(`ng-dirty`,i._shouldForward(`dirty`))(`ng-valid`,i._shouldForward(`valid`))(`ng-invalid`,i._shouldForward(`invalid`))(`ng-pending`,i._shouldForward(`pending`))},inputs:{hideRequiredMarker:`hideRequiredMarker`,color:`color`,floatLabel:`floatLabel`,appearance:`appearance`,subscriptSizing:`subscriptSizing`,hintLabel:`hintLabel`},exportAs:[`matFormField`],features:[qD([{provide:He,useExisting:t},{provide:Be,useExisting:t}])],ngContentSelectors:et,decls:18,vars:21,consts:[[`labelTemplate`,``],[`textField`,``],[`iconPrefixContainer`,``],[`textPrefixContainer`,``],[`textSuffixContainer`,``],[`iconSuffixContainer`,``],[1,`mat-mdc-text-field-wrapper`,`mdc-text-field`,3,`click`],[1,`mat-mdc-form-field-focus-overlay`],[1,`mat-mdc-form-field-flex`],[`matFormFieldNotchedOutline`,``,3,`matFormFieldNotchedOutlineOpen`],[1,`mat-mdc-form-field-icon-prefix`],[1,`mat-mdc-form-field-text-prefix`],[1,`mat-mdc-form-field-infix`],[3,`ngTemplateOutlet`],[1,`mat-mdc-form-field-text-suffix`],[1,`mat-mdc-form-field-icon-suffix`],[`matFormFieldLineRipple`,``],[`aria-atomic`,`true`,`aria-live`,`polite`,1,`mat-mdc-form-field-subscript-wrapper`,`mat-mdc-form-field-bottom-align`],[1,`mat-mdc-form-field-error-wrapper`],[1,`mat-mdc-form-field-hint-wrapper`],[`matFormFieldFloatingLabel`,``,3,`floating`,`monitorResize`,`id`],[`aria-hidden`,`true`,1,`mat-mdc-form-field-required-marker`,`mdc-floating-label--required`],[3,`id`],[1,`mat-mdc-form-field-hint-spacer`]],template:function(n,i){if(n&1&&(aD(Je),Op(0,nt,1,1,`ng-template`,null,0,XD),Ei$1(2,`div`,6,1),zp(`click`,function(l){return i._control.onContainerClick(l)}),$E(4,ot,1,0,`div`,7),Ei$1(5,`div`,8),$E(6,at,2,2,`div`,9),$E(7,dt,3,0,`div`,10),$E(8,mt,3,0,`div`,11),Ei$1(9,`div`,12),$E(10,ft$1,1,1,null,13),cD(11),Hc(),$E(12,st,3,0,`div`,14),$E(13,ut$1,3,0,`div`,15),Hc(),$E(14,pt$1,1,0,`div`,16),Hc(),Ei$1(15,`div`,17),$E(16,ht,2,0,`div`,18)(17,xt,5,1,`div`,19),Hc()),n&2){let o;Bv(2),nh(`mdc-text-field--filled`,!i._hasOutline())(`mdc-text-field--outlined`,i._hasOutline())(`mdc-text-field--no-label`,!i._hasFloatingLabel())(`mdc-text-field--disabled`,i._control.disabled)(`mdc-text-field--invalid`,i._control.errorState),Bv(2),UE(!i._hasOutline()&&!i._control.disabled?4:-1),Bv(2),UE(i._hasOutline()?6:-1),Bv(),UE(i._hasIconPrefix?7:-1),Bv(),UE(i._hasTextPrefix?8:-1),Bv(2),UE(!i._hasOutline()||i._forceDisplayInfixLabel()?10:-1),Bv(2),UE(i._hasTextSuffix?12:-1),Bv(),UE(i._hasIconSuffix?13:-1),Bv(),UE(i._hasOutline()?-1:14),Bv(),nh(`mat-mdc-form-field-subscript-dynamic-size`,i.subscriptSizing===`dynamic`);let l=i._getSubscriptMessageType();Bv(),UE((o=l)===`error`?16:o===`hint`?17:-1)}},dependencies:[Ee,Oe,Js,De,V],styles:[`.mdc-text-field {
  display: inline-flex;
  align-items: baseline;
  padding: 0 16px;
  position: relative;
  box-sizing: border-box;
  overflow: hidden;
  will-change: opacity, transform, color;
  border-top-left-radius: 4px;
  border-top-right-radius: 4px;
  border-bottom-right-radius: 0;
  border-bottom-left-radius: 0;
}

.mdc-text-field__input {
  width: 100%;
  min-width: 0;
  border: none;
  border-radius: 0;
  background: none;
  padding: 0;
  -moz-appearance: none;
  -webkit-appearance: none;
  height: 28px;
}
.mdc-text-field__input::-webkit-calendar-picker-indicator, .mdc-text-field__input::-webkit-search-cancel-button {
  display: none;
}
.mdc-text-field__input::-ms-clear {
  display: none;
}
.mdc-text-field__input:focus {
  outline: none;
}
.mdc-text-field__input:invalid {
  box-shadow: none;
}
.mdc-text-field__input::placeholder {
  opacity: 0;
}
.mdc-text-field__input::-moz-placeholder {
  opacity: 0;
}
.mdc-text-field__input::-webkit-input-placeholder {
  opacity: 0;
}
.mdc-text-field__input:-ms-input-placeholder {
  opacity: 0;
}
.mdc-text-field--no-label .mdc-text-field__input::placeholder, .mdc-text-field--focused .mdc-text-field__input::placeholder {
  opacity: 1;
}
.mdc-text-field--no-label .mdc-text-field__input::-moz-placeholder, .mdc-text-field--focused .mdc-text-field__input::-moz-placeholder {
  opacity: 1;
}
.mdc-text-field--no-label .mdc-text-field__input::-webkit-input-placeholder, .mdc-text-field--focused .mdc-text-field__input::-webkit-input-placeholder {
  opacity: 1;
}
.mdc-text-field--no-label .mdc-text-field__input:-ms-input-placeholder, .mdc-text-field--focused .mdc-text-field__input:-ms-input-placeholder {
  opacity: 1;
}
.mdc-text-field--%NS%disabled:not(.mdc-text-field--no-label) .mdc-text-field__input.mat-mdc-input-disabled-interactive::placeholder {
  opacity: 0;
}
.mdc-text-field--%NS%disabled:not(.mdc-text-field--no-label) .mdc-text-field__input.mat-mdc-input-disabled-interactive::-moz-placeholder {
  opacity: 0;
}
.mdc-text-field--%NS%disabled:not(.mdc-text-field--no-label) .mdc-text-field__input.mat-mdc-input-disabled-interactive::-webkit-input-placeholder {
  opacity: 0;
}
.mdc-text-field--%NS%disabled:not(.mdc-text-field--no-label) .mdc-text-field__input.mat-mdc-input-disabled-interactive:-ms-input-placeholder {
  opacity: 0;
}
.mdc-text-field--outlined .mdc-text-field__input, .mdc-text-field--filled.mdc-text-field--no-label .mdc-text-field__input {
  height: 100%;
}
.mdc-text-field--outlined .mdc-text-field__input {
  display: flex;
  border: none !important;
  background-color: transparent;
}
.mdc-text-field--disabled .mdc-text-field__input {
  pointer-events: auto;
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled) .mdc-text-field__input {
  color: var(--%NS%mat-form-field-filled-input-text-color, var(--%NS%mat-sys-on-surface));
  caret-color: var(--%NS%mat-form-field-filled-caret-color, var(--%NS%mat-sys-primary));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled) .mdc-text-field__input::placeholder {
  color: var(--%NS%mat-form-field-filled-input-text-placeholder-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled) .mdc-text-field__input::-moz-placeholder {
  color: var(--%NS%mat-form-field-filled-input-text-placeholder-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled) .mdc-text-field__input::-webkit-input-placeholder {
  color: var(--%NS%mat-form-field-filled-input-text-placeholder-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled) .mdc-text-field__input:-ms-input-placeholder {
  color: var(--%NS%mat-form-field-filled-input-text-placeholder-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled) .mdc-text-field__input {
  color: var(--%NS%mat-form-field-outlined-input-text-color, var(--%NS%mat-sys-on-surface));
  caret-color: var(--%NS%mat-form-field-outlined-caret-color, var(--%NS%mat-sys-primary));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled) .mdc-text-field__input::placeholder {
  color: var(--%NS%mat-form-field-outlined-input-text-placeholder-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled) .mdc-text-field__input::-moz-placeholder {
  color: var(--%NS%mat-form-field-outlined-input-text-placeholder-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled) .mdc-text-field__input::-webkit-input-placeholder {
  color: var(--%NS%mat-form-field-outlined-input-text-placeholder-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled) .mdc-text-field__input:-ms-input-placeholder {
  color: var(--%NS%mat-form-field-outlined-input-text-placeholder-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--filled.mdc-text-field--%NS%invalid:not(.mdc-text-field--disabled) .mdc-text-field__input {
  caret-color: var(--%NS%mat-form-field-filled-error-caret-color, var(--%NS%mat-sys-error));
}
.mdc-text-field--outlined.mdc-text-field--%NS%invalid:not(.mdc-text-field--disabled) .mdc-text-field__input {
  caret-color: var(--%NS%mat-form-field-outlined-error-caret-color, var(--%NS%mat-sys-error));
}
.mdc-text-field--filled.mdc-text-field--disabled .mdc-text-field__input {
  color: var(--%NS%mat-form-field-filled-disabled-input-text-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mdc-text-field--outlined.mdc-text-field--disabled .mdc-text-field__input {
  color: var(--%NS%mat-form-field-outlined-disabled-input-text-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
@media (forced-colors: active) {
  .mdc-text-field--disabled .mdc-text-field__input {
    background-color: Window;
  }
}

.mdc-text-field--filled {
  height: 56px;
  border-bottom-right-radius: 0;
  border-bottom-left-radius: 0;
  border-top-left-radius: var(--%NS%mat-form-field-filled-container-shape, var(--%NS%mat-sys-corner-extra-small));
  border-top-right-radius: var(--%NS%mat-form-field-filled-container-shape, var(--%NS%mat-sys-corner-extra-small));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled) {
  background-color: var(--%NS%mat-form-field-filled-container-color, var(--%NS%mat-sys-surface-variant));
}
.mdc-text-field--filled.mdc-text-field--disabled {
  background-color: var(--%NS%mat-form-field-filled-disabled-container-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 4%, transparent));
}

.mdc-text-field--outlined {
  height: 56px;
  overflow: visible;
  padding-right: max(16px, var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small)));
  padding-left: max(16px, var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small)) + 4px);
}
[dir=rtl] .mdc-text-field--outlined {
  padding-right: max(16px, var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small)) + 4px);
  padding-left: max(16px, var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small)));
}

.mdc-floating-label {
  position: absolute;
  left: 0;
  transform-origin: left top;
  line-height: 1.15rem;
  text-align: left;
  text-overflow: ellipsis;
  white-space: nowrap;
  cursor: text;
  overflow: hidden;
  will-change: transform;
}
[dir=rtl] .mdc-floating-label {
  right: 0;
  left: auto;
  transform-origin: right top;
  text-align: right;
}
.mdc-text-field .mdc-floating-label {
  top: 50%;
  transform: translateY(-50%);
  pointer-events: none;
}
.mdc-notched-outline .mdc-floating-label {
  display: inline-block;
  position: relative;
  max-width: 100%;
}
.mdc-text-field--outlined .mdc-floating-label {
  left: 4px;
  right: auto;
}
[dir=rtl] .mdc-text-field--outlined .mdc-floating-label {
  left: auto;
  right: 4px;
}
.mdc-text-field--filled .mdc-floating-label {
  left: 16px;
  right: auto;
}
[dir=rtl] .mdc-text-field--filled .mdc-floating-label {
  left: auto;
  right: 16px;
}
.mdc-text-field--disabled .mdc-floating-label {
  cursor: default;
}
@media (forced-colors: active) {
  .mdc-text-field--disabled .mdc-floating-label {
    z-index: 1;
  }
}
.mdc-text-field--filled.mdc-text-field--no-label .mdc-floating-label {
  display: none;
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled) .mdc-floating-label {
  color: var(--%NS%mat-form-field-filled-label-text-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled).mdc-text-field--focused .mdc-floating-label {
  color: var(--%NS%mat-form-field-filled-focus-label-text-color, var(--%NS%mat-sys-primary));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled):not(.mdc-text-field--focused):hover .mdc-floating-label {
  color: var(--%NS%mat-form-field-filled-hover-label-text-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--filled.mdc-text-field--disabled .mdc-floating-label {
  color: var(--%NS%mat-form-field-filled-disabled-label-text-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled).mdc-text-field--invalid .mdc-floating-label {
  color: var(--%NS%mat-form-field-filled-error-label-text-color, var(--%NS%mat-sys-error));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled).mdc-text-field--invalid.mdc-text-field--focused .mdc-floating-label {
  color: var(--%NS%mat-form-field-filled-error-focus-label-text-color, var(--%NS%mat-sys-error));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled).mdc-text-field--%NS%invalid:not(.mdc-text-field--disabled):hover .mdc-floating-label {
  color: var(--%NS%mat-form-field-filled-error-hover-label-text-color, var(--%NS%mat-sys-on-error-container));
}
.mdc-text-field--filled .mdc-floating-label {
  font-family: var(--%NS%mat-form-field-filled-label-text-font, var(--%NS%mat-sys-body-large-font));
  font-size: var(--%NS%mat-form-field-filled-label-text-size, var(--%NS%mat-sys-body-large-size));
  font-weight: var(--%NS%mat-form-field-filled-label-text-weight, var(--%NS%mat-sys-body-large-weight));
  letter-spacing: var(--%NS%mat-form-field-filled-label-text-tracking, var(--%NS%mat-sys-body-large-tracking));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled) .mdc-floating-label {
  color: var(--%NS%mat-form-field-outlined-label-text-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled).mdc-text-field--focused .mdc-floating-label {
  color: var(--%NS%mat-form-field-outlined-focus-label-text-color, var(--%NS%mat-sys-primary));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled):not(.mdc-text-field--focused):hover .mdc-floating-label {
  color: var(--%NS%mat-form-field-outlined-hover-label-text-color, var(--%NS%mat-sys-on-surface));
}
.mdc-text-field--outlined.mdc-text-field--disabled .mdc-floating-label {
  color: var(--%NS%mat-form-field-outlined-disabled-label-text-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled).mdc-text-field--invalid .mdc-floating-label {
  color: var(--%NS%mat-form-field-outlined-error-label-text-color, var(--%NS%mat-sys-error));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled).mdc-text-field--invalid.mdc-text-field--focused .mdc-floating-label {
  color: var(--%NS%mat-form-field-outlined-error-focus-label-text-color, var(--%NS%mat-sys-error));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled).mdc-text-field--%NS%invalid:not(.mdc-text-field--disabled):hover .mdc-floating-label {
  color: var(--%NS%mat-form-field-outlined-error-hover-label-text-color, var(--%NS%mat-sys-on-error-container));
}
.mdc-text-field--outlined .mdc-floating-label {
  font-family: var(--%NS%mat-form-field-outlined-label-text-font, var(--%NS%mat-sys-body-large-font));
  font-size: var(--%NS%mat-form-field-outlined-label-text-size, var(--%NS%mat-sys-body-large-size));
  font-weight: var(--%NS%mat-form-field-outlined-label-text-weight, var(--%NS%mat-sys-body-large-weight));
  letter-spacing: var(--%NS%mat-form-field-outlined-label-text-tracking, var(--%NS%mat-sys-body-large-tracking));
}

.mdc-floating-label--float-above {
  cursor: auto;
  transform: translateY(-106%) scale(0.75);
}
.mdc-text-field--filled .mdc-floating-label--float-above {
  transform: translateY(-106%) scale(0.75);
}
.mdc-text-field--outlined .mdc-floating-label--float-above {
  transform: translateY(-37.25px) scale(1);
  font-size: 0.75rem;
}
.mdc-notched-outline .mdc-floating-label--float-above {
  text-overflow: clip;
}
.mdc-notched-outline--upgraded .mdc-floating-label--float-above {
  max-width: 133.3333333333%;
}
.mdc-text-field--outlined.mdc-notched-outline--upgraded .mdc-floating-label--float-above, .mdc-text-field--outlined .mdc-notched-outline--upgraded .mdc-floating-label--float-above {
  transform: translateY(-34.75px) scale(0.75);
}
.mdc-text-field--outlined.mdc-notched-outline--upgraded .mdc-floating-label--float-above, .mdc-text-field--outlined .mdc-notched-outline--upgraded .mdc-floating-label--float-above {
  font-size: 1rem;
}

.mdc-floating-label--%NS%required:not(.mdc-floating-label--hide-required-marker)::after {
  margin-left: 1px;
  margin-right: 0;
  content: "*";
}
[dir=rtl] .mdc-floating-label--%NS%required:not(.mdc-floating-label--hide-required-marker)::after {
  margin-left: 0;
  margin-right: 1px;
}

.mdc-notched-outline {
  display: flex;
  position: absolute;
  top: 0;
  right: 0;
  left: 0;
  box-sizing: border-box;
  width: 100%;
  max-width: 100%;
  height: 100%;
  text-align: left;
  pointer-events: none;
}
[dir=rtl] .mdc-notched-outline {
  text-align: right;
}
.mdc-text-field--outlined .mdc-notched-outline {
  z-index: 1;
}

.mat-mdc-notch-piece {
  box-sizing: border-box;
  height: 100%;
  pointer-events: none;
  border: none;
  border-top: 1px solid;
  border-bottom: 1px solid;
}
.mdc-text-field--focused .mat-mdc-notch-piece {
  border-width: 2px;
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled) .mat-mdc-notch-piece {
  border-color: var(--%NS%mat-form-field-outlined-outline-color, var(--%NS%mat-sys-outline));
  border-width: var(--%NS%mat-form-field-outlined-outline-width, 1px);
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled):not(.mdc-text-field--focused):hover .mat-mdc-notch-piece {
  border-color: var(--%NS%mat-form-field-outlined-hover-outline-color, var(--%NS%mat-sys-on-surface));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled).mdc-text-field--focused .mat-mdc-notch-piece {
  border-color: var(--%NS%mat-form-field-outlined-focus-outline-color, var(--%NS%mat-sys-primary));
}
.mdc-text-field--outlined.mdc-text-field--disabled .mat-mdc-notch-piece {
  border-color: var(--%NS%mat-form-field-outlined-disabled-outline-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 12%, transparent));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled).mdc-text-field--invalid .mat-mdc-notch-piece {
  border-color: var(--%NS%mat-form-field-outlined-error-outline-color, var(--%NS%mat-sys-error));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled).mdc-text-field--%NS%invalid:not(.mdc-text-field--focused):hover .mdc-notched-outline .mat-mdc-notch-piece {
  border-color: var(--%NS%mat-form-field-outlined-error-hover-outline-color, var(--%NS%mat-sys-on-error-container));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled).mdc-text-field--invalid.mdc-text-field--focused .mat-mdc-notch-piece {
  border-color: var(--%NS%mat-form-field-outlined-error-focus-outline-color, var(--%NS%mat-sys-error));
}
.mdc-text-field--%NS%outlined:not(.mdc-text-field--disabled).mdc-text-field--focused .mdc-notched-outline .mat-mdc-notch-piece {
  border-width: var(--%NS%mat-form-field-outlined-focus-outline-width, 2px);
}

.mdc-notched-outline__leading {
  border-left: 1px solid;
  border-right: none;
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
  border-top-left-radius: var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small));
  border-bottom-left-radius: var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small));
}
.mdc-text-field--outlined .mdc-notched-outline .mdc-notched-outline__leading {
  width: max(12px, var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small)));
}
[dir=rtl] .mdc-notched-outline__leading {
  border-left: none;
  border-right: 1px solid;
  border-bottom-left-radius: 0;
  border-top-left-radius: 0;
  border-top-right-radius: var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small));
  border-bottom-right-radius: var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small));
}

.mdc-notched-outline__trailing {
  flex-grow: 1;
  border-left: none;
  border-right: 1px solid;
  border-top-left-radius: 0;
  border-bottom-left-radius: 0;
  border-top-right-radius: var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small));
  border-bottom-right-radius: var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small));
}
[dir=rtl] .mdc-notched-outline__trailing {
  border-left: 1px solid;
  border-right: none;
  border-top-right-radius: 0;
  border-bottom-right-radius: 0;
  border-top-left-radius: var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small));
  border-bottom-left-radius: var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small));
}

.mdc-notched-outline__notch {
  flex: 0 0 auto;
  width: auto;
}
.mdc-text-field--outlined .mdc-notched-outline .mdc-notched-outline__notch {
  max-width: min(var(--%NS%mat-form-field-notch-max-width, 100%), calc(100% - max(12px, var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small))) * 2));
}
.mdc-text-field--outlined .mdc-notched-outline--notched .mdc-notched-outline__notch {
  max-width: min(100%, calc(100% - max(12px, var(--%NS%mat-form-field-outlined-container-shape, var(--%NS%mat-sys-corner-extra-small))) * 2));
}
.mdc-text-field--outlined .mdc-notched-outline--notched .mdc-notched-outline__notch {
  padding-top: 1px;
}
.mdc-text-field--focused.mdc-text-field--outlined .mdc-notched-outline--notched .mdc-notched-outline__notch {
  padding-top: 2px;
}
.mdc-notched-outline--notched .mdc-notched-outline__notch {
  padding-left: 0;
  padding-right: 8px;
  border-top: none;
}
[dir=rtl] .mdc-notched-outline--notched .mdc-notched-outline__notch {
  padding-left: 8px;
  padding-right: 0;
}
.mdc-notched-outline--no-label .mdc-notched-outline__notch {
  display: none;
}

.mdc-line-ripple::before, .mdc-line-ripple::after {
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  border-bottom-style: solid;
  content: "";
}
.mdc-line-ripple::before {
  z-index: 1;
  border-bottom-width: var(--%NS%mat-form-field-filled-active-indicator-height, 1px);
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled) .mdc-line-ripple::before {
  border-bottom-color: var(--%NS%mat-form-field-filled-active-indicator-color, var(--%NS%mat-sys-on-surface-variant));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled):not(.mdc-text-field--focused):hover .mdc-line-ripple::before {
  border-bottom-color: var(--%NS%mat-form-field-filled-hover-active-indicator-color, var(--%NS%mat-sys-on-surface));
}
.mdc-text-field--filled.mdc-text-field--disabled .mdc-line-ripple::before {
  border-bottom-color: var(--%NS%mat-form-field-filled-disabled-active-indicator-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled).mdc-text-field--invalid .mdc-line-ripple::before {
  border-bottom-color: var(--%NS%mat-form-field-filled-error-active-indicator-color, var(--%NS%mat-sys-error));
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled).mdc-text-field--%NS%invalid:not(.mdc-text-field--focused):hover .mdc-line-ripple::before {
  border-bottom-color: var(--%NS%mat-form-field-filled-error-hover-active-indicator-color, var(--%NS%mat-sys-on-error-container));
}
.mdc-line-ripple::after {
  transform: scaleX(0);
  opacity: 0;
  z-index: 2;
}
.mdc-text-field--filled .mdc-line-ripple::after {
  border-bottom-width: var(--%NS%mat-form-field-filled-focus-active-indicator-height, 2px);
}
.mdc-text-field--%NS%filled:not(.mdc-text-field--disabled) .mdc-line-ripple::after {
  border-bottom-color: var(--%NS%mat-form-field-filled-focus-active-indicator-color, var(--%NS%mat-sys-primary));
}
.mdc-text-field--filled.mdc-text-field--%NS%invalid:not(.mdc-text-field--disabled) .mdc-line-ripple::after {
  border-bottom-color: var(--%NS%mat-form-field-filled-error-focus-active-indicator-color, var(--%NS%mat-sys-error));
}

.mdc-line-ripple--%NS%active::after {
  transform: scaleX(1);
  opacity: 1;
}

.mdc-line-ripple--%NS%deactivating::after {
  opacity: 0;
}

.mdc-text-field--disabled {
  pointer-events: none;
}

.mat-mdc-form-field-textarea-control {
  vertical-align: middle;
  resize: vertical;
  box-sizing: border-box;
  height: auto;
  margin: 0;
  padding: 0;
  border: none;
  overflow: auto;
}

.mat-mdc-form-field-input-control.mat-mdc-form-field-input-control {
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  font: inherit;
  letter-spacing: inherit;
  text-decoration: inherit;
  text-transform: inherit;
  border: none;
}

.mat-mdc-form-field .mat-mdc-floating-label.mdc-floating-label {
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  line-height: normal;
  pointer-events: all;
  will-change: auto;
}

.mat-mdc-form-field:not(.mat-form-field-disabled) .mat-mdc-floating-label.mdc-floating-label {
  cursor: inherit;
}

.mdc-text-field--%NS%no-label:not(.mdc-text-field--textarea) .mat-mdc-form-field-input-control.mdc-text-field__input,
.mat-mdc-text-field-wrapper .mat-mdc-form-field-input-control {
  height: auto;
}

.mat-mdc-text-field-wrapper .mat-mdc-form-field-input-control.mdc-text-field__input[type=color] {
  height: 23px;
}

.mat-mdc-text-field-wrapper {
  height: auto;
  flex: auto;
  will-change: auto;
}

.mat-mdc-form-field-has-icon-prefix .mat-mdc-text-field-wrapper {
  padding-left: 0;
  --%NS%mat-mdc-form-field-label-offset-x: -16px;
}

.mat-mdc-form-field-has-icon-suffix .mat-mdc-text-field-wrapper {
  padding-right: 0;
}

[dir=rtl] .mat-mdc-text-field-wrapper {
  padding-left: 16px;
  padding-right: 16px;
}
[dir=rtl] .mat-mdc-form-field-has-icon-suffix .mat-mdc-text-field-wrapper {
  padding-left: 0;
}
[dir=rtl] .mat-mdc-form-field-has-icon-prefix .mat-mdc-text-field-wrapper {
  padding-right: 0;
}

.mat-form-field-disabled .mdc-text-field__input::placeholder {
  color: var(--%NS%mat-form-field-disabled-input-text-placeholder-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mat-form-field-disabled .mdc-text-field__input::-moz-placeholder {
  color: var(--%NS%mat-form-field-disabled-input-text-placeholder-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mat-form-field-disabled .mdc-text-field__input::-webkit-input-placeholder {
  color: var(--%NS%mat-form-field-disabled-input-text-placeholder-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mat-form-field-disabled .mdc-text-field__input:-ms-input-placeholder {
  color: var(--%NS%mat-form-field-disabled-input-text-placeholder-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}

.mat-mdc-form-field-label-always-float .mdc-text-field__input::placeholder {
  transition-delay: 40ms;
  transition-duration: 110ms;
  opacity: 1;
}

.mat-mdc-text-field-wrapper .mat-mdc-form-field-infix .mat-mdc-floating-label {
  left: auto;
  right: auto;
}

.mat-mdc-text-field-wrapper.mdc-text-field--outlined .mdc-text-field__input {
  display: inline-block;
}

.mat-mdc-form-field .mat-mdc-text-field-wrapper.mdc-text-field .mdc-notched-outline__notch {
  padding-top: 0;
}

.mat-mdc-form-field.mat-mdc-form-field.mat-mdc-form-field.mat-mdc-form-field.mat-mdc-form-field.mat-mdc-form-field .mdc-notched-outline__notch {
  border-left: 1px solid transparent;
}

[dir=rtl] .mat-mdc-form-field.mat-mdc-form-field.mat-mdc-form-field.mat-mdc-form-field.mat-mdc-form-field.mat-mdc-form-field .mdc-notched-outline__notch {
  border-left: none;
  border-right: 1px solid transparent;
}

.mat-mdc-form-field-infix {
  min-height: var(--%NS%mat-form-field-container-height, 56px);
  padding-top: var(--%NS%mat-form-field-filled-with-label-container-padding-top, 24px);
  padding-bottom: var(--%NS%mat-form-field-filled-with-label-container-padding-bottom, 8px);
}
.mdc-text-field--outlined .mat-mdc-form-field-infix, .mdc-text-field--no-label .mat-mdc-form-field-infix {
  padding-top: var(--%NS%mat-form-field-container-vertical-padding, 16px);
  padding-bottom: var(--%NS%mat-form-field-container-vertical-padding, 16px);
}

.mat-mdc-text-field-wrapper .mat-mdc-form-field-flex .mat-mdc-floating-label {
  top: calc(var(--%NS%mat-form-field-container-height, 56px) / 2);
}

.mdc-text-field--filled .mat-mdc-floating-label {
  display: var(--%NS%mat-form-field-filled-label-display, block);
}

.mat-mdc-text-field-wrapper.mdc-text-field--outlined .mdc-notched-outline--upgraded .mdc-floating-label--float-above {
  --%NS%mat-mdc-form-field-label-transform: translateY(calc(calc(6.75px + var(--%NS%mat-form-field-container-height, 56px) / 2) * -1))
    scale(var(--%NS%mat-mdc-form-field-floating-label-scale, 0.75));
  transform: var(--%NS%mat-mdc-form-field-label-transform);
}

@keyframes _mat-form-field-subscript-animation {
  from {
    opacity: 0;
    transform: translateY(-5px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
.mat-mdc-form-field-subscript-wrapper {
  box-sizing: border-box;
  width: 100%;
  position: relative;
}

.mat-mdc-form-field-hint-wrapper,
.mat-mdc-form-field-error-wrapper {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  padding: 0 16px;
  opacity: 1;
  transform: translateY(0);
  animation: _mat-form-field-subscript-animation 0ms cubic-bezier(0.55, 0, 0.55, 0.2);
}

.mat-mdc-form-field-subscript-dynamic-size .mat-mdc-form-field-hint-wrapper,
.mat-mdc-form-field-subscript-dynamic-size .mat-mdc-form-field-error-wrapper {
  position: static;
}

.mat-mdc-form-field-bottom-align::before {
  content: "";
  display: inline-block;
  height: 16px;
}

.mat-mdc-form-field-bottom-align.mat-mdc-form-field-subscript-dynamic-size::before {
  content: unset;
}

.mat-mdc-form-field-hint-end {
  order: 1;
}

.mat-mdc-form-field-hint-wrapper {
  display: flex;
}

.mat-mdc-form-field-hint-spacer {
  flex: 1 0 1em;
}

.mat-mdc-form-field-error {
  display: block;
  color: var(--%NS%mat-form-field-error-text-color, var(--%NS%mat-sys-error));
}

.mat-mdc-form-field-subscript-wrapper,
.mat-mdc-form-field-bottom-align::before {
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  font-family: var(--%NS%mat-form-field-subscript-text-font, var(--%NS%mat-sys-body-small-font));
  line-height: var(--%NS%mat-form-field-subscript-text-line-height, var(--%NS%mat-sys-body-small-line-height));
  font-size: var(--%NS%mat-form-field-subscript-text-size, var(--%NS%mat-sys-body-small-size));
  letter-spacing: var(--%NS%mat-form-field-subscript-text-tracking, var(--%NS%mat-sys-body-small-tracking));
  font-weight: var(--%NS%mat-form-field-subscript-text-weight, var(--%NS%mat-sys-body-small-weight));
}

.mat-mdc-form-field-focus-overlay {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  opacity: 0;
  pointer-events: none;
  background-color: var(--%NS%mat-form-field-state-layer-color, var(--%NS%mat-sys-on-surface));
}
.mat-mdc-text-field-wrapper:hover .mat-mdc-form-field-focus-overlay {
  opacity: var(--%NS%mat-form-field-hover-state-layer-opacity, var(--%NS%mat-sys-hover-state-layer-opacity));
}
.mat-mdc-form-field.mat-focused .mat-mdc-form-field-focus-overlay {
  opacity: var(--%NS%mat-form-field-focus-state-layer-opacity, 0);
}

select.mat-mdc-form-field-input-control {
  -moz-appearance: none;
  -webkit-appearance: none;
  background-color: transparent;
  display: inline-flex;
  box-sizing: border-box;
}
select.mat-mdc-form-field-input-control:not(:disabled) {
  cursor: pointer;
}
select.mat-mdc-form-field-input-control:not(.mat-mdc-native-select-inline) option {
  color: var(--%NS%mat-form-field-select-option-text-color, var(--%NS%mat-sys-neutral10));
}
select.mat-mdc-form-field-input-control:not(.mat-mdc-native-select-inline) option:disabled {
  color: var(--%NS%mat-form-field-select-disabled-option-text-color, color-mix(in srgb, var(--%NS%mat-sys-neutral10) 38%, transparent));
}

.mat-mdc-form-field-type-mat-native-select .mat-mdc-form-field-infix::after {
  content: "";
  width: 0;
  height: 0;
  border-left: 5px solid transparent;
  border-right: 5px solid transparent;
  border-top: 5px solid;
  position: absolute;
  right: 0;
  top: 50%;
  margin-top: -2.5px;
  pointer-events: none;
  color: var(--%NS%mat-form-field-enabled-select-arrow-color, var(--%NS%mat-sys-on-surface-variant));
}
[dir=rtl] .mat-mdc-form-field-type-mat-native-select .mat-mdc-form-field-infix::after {
  right: auto;
  left: 0;
}
.mat-mdc-form-field-type-mat-native-select.mat-focused .mat-mdc-form-field-infix::after {
  color: var(--%NS%mat-form-field-focus-select-arrow-color, var(--%NS%mat-sys-primary));
}
.mat-mdc-form-field-type-mat-native-select.mat-form-field-disabled .mat-mdc-form-field-infix::after {
  color: var(--%NS%mat-form-field-disabled-select-arrow-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mat-mdc-form-field-type-mat-native-select .mat-mdc-form-field-input-control {
  padding-right: 15px;
}
[dir=rtl] .mat-mdc-form-field-type-mat-native-select .mat-mdc-form-field-input-control {
  padding-right: 0;
  padding-left: 15px;
}

@media (forced-colors: active) {
  .mat-form-field-appearance-fill .mat-mdc-text-field-wrapper {
    outline: solid 1px;
  }
}
@media (forced-colors: active) {
  .mat-form-field-appearance-fill.mat-form-field-disabled .mat-mdc-text-field-wrapper {
    outline-color: GrayText;
  }
}

@media (forced-colors: active) {
  .mat-form-field-appearance-fill.mat-focused .mat-mdc-text-field-wrapper {
    outline: dashed 3px;
  }
}

@media (forced-colors: active) {
  .mat-mdc-form-field.mat-focused .mdc-notched-outline {
    border: dashed 3px;
  }
}

.mat-mdc-form-field-input-control[type=date], .mat-mdc-form-field-input-control[type=datetime], .mat-mdc-form-field-input-control[type=datetime-local], .mat-mdc-form-field-input-control[type=month], .mat-mdc-form-field-input-control[type=week], .mat-mdc-form-field-input-control[type=time] {
  line-height: 1;
}
.mat-mdc-form-field-input-control::-webkit-datetime-edit {
  line-height: 1;
  padding: 0;
  margin-bottom: -2px;
}

.mat-mdc-form-field {
  --%NS%mat-mdc-form-field-floating-label-scale: 0.75;
  display: inline-flex;
  flex-direction: column;
  min-width: 0;
  text-align: left;
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  font-family: var(--%NS%mat-form-field-container-text-font, var(--%NS%mat-sys-body-large-font));
  line-height: var(--%NS%mat-form-field-container-text-line-height, var(--%NS%mat-sys-body-large-line-height));
  font-size: var(--%NS%mat-form-field-container-text-size, var(--%NS%mat-sys-body-large-size));
  letter-spacing: var(--%NS%mat-form-field-container-text-tracking, var(--%NS%mat-sys-body-large-tracking));
  font-weight: var(--%NS%mat-form-field-container-text-weight, var(--%NS%mat-sys-body-large-weight));
}
.mat-mdc-form-field .mdc-text-field--outlined .mdc-floating-label--float-above {
  font-size: calc(var(--%NS%mat-form-field-outlined-label-text-populated-size) * var(--%NS%mat-mdc-form-field-floating-label-scale));
}
.mat-mdc-form-field .mdc-text-field--outlined .mdc-notched-outline--upgraded .mdc-floating-label--float-above {
  font-size: var(--%NS%mat-form-field-outlined-label-text-populated-size);
}
[dir=rtl] .mat-mdc-form-field {
  text-align: right;
}

.mat-mdc-form-field-flex {
  display: inline-flex;
  align-items: baseline;
  box-sizing: border-box;
  width: 100%;
}

.mat-mdc-text-field-wrapper {
  width: 100%;
  z-index: 0;
}

.mat-mdc-form-field-icon-prefix,
.mat-mdc-form-field-icon-suffix {
  align-self: center;
  line-height: 0;
  pointer-events: auto;
  position: relative;
  z-index: 1;
}
.mat-mdc-form-field-icon-prefix > .mat-icon,
.mat-mdc-form-field-icon-suffix > .mat-icon {
  padding: 0 12px;
  box-sizing: content-box;
}

.mat-mdc-form-field-icon-prefix {
  color: var(--%NS%mat-form-field-leading-icon-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-form-field-disabled .mat-mdc-form-field-icon-prefix {
  color: var(--%NS%mat-form-field-disabled-leading-icon-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}

.mat-mdc-form-field-icon-suffix {
  color: var(--%NS%mat-form-field-trailing-icon-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-form-field-disabled .mat-mdc-form-field-icon-suffix {
  color: var(--%NS%mat-form-field-disabled-trailing-icon-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mat-form-field-invalid .mat-mdc-form-field-icon-suffix {
  color: var(--%NS%mat-form-field-error-trailing-icon-color, var(--%NS%mat-sys-error));
}
.mat-form-field-invalid:not(.mat-focused):not(.mat-form-field-disabled) .mat-mdc-text-field-wrapper:hover .mat-mdc-form-field-icon-suffix {
  color: var(--%NS%mat-form-field-error-hover-trailing-icon-color, var(--%NS%mat-sys-on-error-container));
}
.mat-form-field-invalid.mat-focused .mat-mdc-text-field-wrapper .mat-mdc-form-field-icon-suffix {
  color: var(--%NS%mat-form-field-error-focus-trailing-icon-color, var(--%NS%mat-sys-error));
}

.mat-mdc-form-field-icon-prefix,
[dir=rtl] .mat-mdc-form-field-icon-suffix {
  padding: 0 4px 0 0;
}

.mat-mdc-form-field-icon-suffix,
[dir=rtl] .mat-mdc-form-field-icon-prefix {
  padding: 0 0 0 4px;
}

.mat-mdc-form-field-subscript-wrapper .mat-icon,
.mat-mdc-form-field label .mat-icon {
  width: 1em;
  height: 1em;
  font-size: inherit;
}

.mat-mdc-form-field-infix {
  flex: auto;
  min-width: 0;
  width: 180px;
  position: relative;
  box-sizing: border-box;
}
.mat-mdc-form-field-infix:has(textarea[cols]) {
  width: auto;
}

.mat-mdc-form-field .mdc-notched-outline__notch {
  margin-left: -1px;
  -webkit-clip-path: inset(-9em -999em -9em 1px);
  clip-path: inset(-9em -999em -9em 1px);
}
[dir=rtl] .mat-mdc-form-field .mdc-notched-outline__notch {
  margin-left: 0;
  margin-right: -1px;
  -webkit-clip-path: inset(-9em 1px -9em -999em);
  clip-path: inset(-9em 1px -9em -999em);
}

.mat-mdc-form-field.mat-form-field-animations-enabled .mdc-floating-label {
  transition: transform 150ms cubic-bezier(0.4, 0, 0.2, 1), color 150ms cubic-bezier(0.4, 0, 0.2, 1);
}
.mat-mdc-form-field.mat-form-field-animations-enabled .mdc-text-field__input {
  transition: opacity 150ms cubic-bezier(0.4, 0, 0.2, 1);
}
.mat-mdc-form-field.mat-form-field-animations-enabled .mdc-text-field__input::placeholder {
  transition: opacity 67ms cubic-bezier(0.4, 0, 0.2, 1);
}
.mat-mdc-form-field.mat-form-field-animations-enabled .mdc-text-field__input::-moz-placeholder {
  transition: opacity 67ms cubic-bezier(0.4, 0, 0.2, 1);
}
.mat-mdc-form-field.mat-form-field-animations-enabled .mdc-text-field__input::-webkit-input-placeholder {
  transition: opacity 67ms cubic-bezier(0.4, 0, 0.2, 1);
}
.mat-mdc-form-field.mat-form-field-animations-enabled .mdc-text-field__input:-ms-input-placeholder {
  transition: opacity 67ms cubic-bezier(0.4, 0, 0.2, 1);
}
.mat-mdc-form-field.mat-form-field-animations-enabled.mdc-text-field--no-label .mdc-text-field__input::placeholder, .mat-mdc-form-field.mat-form-field-animations-enabled.mdc-text-field--focused .mdc-text-field__input::placeholder {
  transition-delay: 40ms;
  transition-duration: 110ms;
}
.mat-mdc-form-field.mat-form-field-animations-enabled.mdc-text-field--no-label .mdc-text-field__input::-moz-placeholder, .mat-mdc-form-field.mat-form-field-animations-enabled.mdc-text-field--focused .mdc-text-field__input::-moz-placeholder {
  transition-delay: 40ms;
  transition-duration: 110ms;
}
.mat-mdc-form-field.mat-form-field-animations-enabled.mdc-text-field--no-label .mdc-text-field__input::-webkit-input-placeholder, .mat-mdc-form-field.mat-form-field-animations-enabled.mdc-text-field--focused .mdc-text-field__input::-webkit-input-placeholder {
  transition-delay: 40ms;
  transition-duration: 110ms;
}
.mat-mdc-form-field.mat-form-field-animations-enabled.mdc-text-field--no-label .mdc-text-field__input:-ms-input-placeholder, .mat-mdc-form-field.mat-form-field-animations-enabled.mdc-text-field--focused .mdc-text-field__input:-ms-input-placeholder {
  transition-delay: 40ms;
  transition-duration: 110ms;
}
.mat-mdc-form-field.mat-form-field-animations-enabled .mdc-text-field--%NS%filled:not(.mdc-ripple-upgraded):focus .mdc-text-field__ripple::before {
  transition-duration: 75ms;
}
.mat-mdc-form-field.mat-form-field-animations-enabled .mdc-line-ripple::after {
  transition: transform 180ms cubic-bezier(0.4, 0, 0.2, 1), opacity 180ms cubic-bezier(0.4, 0, 0.2, 1);
}
.mat-mdc-form-field.mat-form-field-animations-enabled .mat-mdc-form-field-hint-wrapper,
.mat-mdc-form-field.mat-form-field-animations-enabled .mat-mdc-form-field-error-wrapper {
  animation-duration: 300ms;
}

.mdc-notched-outline .mdc-floating-label {
  max-width: calc(100% + 1px);
}

.mdc-notched-outline--upgraded .mdc-floating-label--float-above {
  max-width: calc(133.3333333333% + 1px);
}
`],encapsulation:2})}return t})();var ti$1=(()=>{class t{static ɵfac=function(n){return new(n||t)};static ɵmod=IE({type:t});static ɵinj=Jl({imports:[Oe$1,je,yt$1]})}return t})();var yi=`@`;var _i=(()=>{class e{doc;delegate;zone;animationType;moduleImpl;_rendererFactoryPromise=null;scheduler=null;injector=T$1(ge);loadingSchedulerFn=T$1(vi,{optional:!0});_engine;constructor(t,n,i,a,r){this.doc=t,this.delegate=n,this.zone=i,this.animationType=a,this.moduleImpl=r}ngOnDestroy(){this._engine?.flush()}loadImpl(){let t=()=>this.moduleImpl??import(`./chunk-Bi2Akzbd.js`).then(i=>i),n;return this.loadingSchedulerFn?n=this.loadingSchedulerFn(t):n=t(),n.catch(i=>{throw new M$1(5300,!1)}).then(({ɵcreateEngine:i,ɵAnimationRendererFactory:a})=>{this._engine=i(this.animationType,this.doc);let r=new a(this.delegate,this._engine,this.zone);return this.delegate=r,r})}createRenderer(t,n){let i=this.delegate.createRenderer(t,n);if(i.ɵtype===0)return i;typeof i.throwOnSyntheticProps==`boolean`&&(i.throwOnSyntheticProps=!1);let a=new Jt(i);return n?.data?.animation&&!this._rendererFactoryPromise&&(this._rendererFactoryPromise=this.loadImpl()),this._rendererFactoryPromise?.then(r=>{let d=r.createRenderer(t,n);a.use(d),this.scheduler??=this.injector.get(Ae$1,null,{optional:!0}),this.scheduler?.notify(10)}).catch(r=>{a.use(i)}),a}begin(){this.delegate.begin?.()}end(){this.delegate.end?.()}whenRenderingDone(){return this.delegate.whenRenderingDone?.()??Promise.resolve()}componentReplaced(t){this._engine?.flush(),this.delegate.componentReplaced?.(t)}static ɵfac=function(n){LI()};static ɵprov=ie$2({token:e,factory:e.ɵfac})}return e})();var Jt=class{delegate;replay=[];ɵtype=1;constructor(o){this.delegate=o}use(o){if(this.delegate=o,this.replay!==null){for(let t of this.replay)t(o);this.replay=null}}get data(){return this.delegate.data}destroy(){this.replay=null,this.delegate.destroy()}createElement(o,t){return this.delegate.createElement(o,t)}createComment(o){return this.delegate.createComment(o)}createText(o){return this.delegate.createText(o)}get destroyNode(){return this.delegate.destroyNode}appendChild(o,t){this.delegate.appendChild(o,t)}insertBefore(o,t,n,i){this.delegate.insertBefore(o,t,n,i)}removeChild(o,t,n,i){this.delegate.removeChild(o,t,n,i)}selectRootElement(o,t){return this.delegate.selectRootElement(o,t)}parentNode(o){return this.delegate.parentNode(o)}nextSibling(o){return this.delegate.nextSibling(o)}setAttribute(o,t,n,i){this.delegate.setAttribute(o,t,n,i)}removeAttribute(o,t,n){this.delegate.removeAttribute(o,t,n)}addClass(o,t){this.delegate.addClass(o,t)}removeClass(o,t){this.delegate.removeClass(o,t)}setStyle(o,t,n,i){this.delegate.setStyle(o,t,n,i)}removeStyle(o,t,n){this.delegate.removeStyle(o,t,n)}setProperty(o,t,n){this.shouldReplay(t)&&this.replay.push(i=>i.setProperty(o,t,n)),this.delegate.setProperty(o,t,n)}setValue(o,t){this.delegate.setValue(o,t)}listen(o,t,n,i){return this.shouldReplay(t)&&this.replay.push(a=>a.listen(o,t,n,i)),this.delegate.listen(o,t,n,i)}shouldReplay(o){return this.replay!==null&&o.startsWith(yi)}};var vi=new A$2(``);function jn(e=`animations`){return gt$2(`NgAsyncAnimations`),_o([{provide:gr,useFactory:()=>new _i(T$1(or),T$1(Tr),T$1(xe),e)},{provide:vm,useValue:e===`noop`?`NoopAnimations`:`BrowserAnimations`}])}var ut=`PERFORM_ACTION`;var xi=`REFRESH`;var Vn=`RESET`;var qn=`ROLLBACK`;var Gn=`COMMIT`;var Zn=`SWEEP`;var Xn=`TOGGLE_ACTION`;var Si=`SET_ACTIONS_ACTIVE`;var Kn=`JUMP_TO_STATE`;var Qn=`JUMP_TO_ACTION`;var pe=`IMPORT_STATE`;var Wn=`LOCK_CHANGES`;var Yn=`PAUSE_RECORDING`;var J=class{constructor(o,t){if(this.action=o,this.timestamp=t,this.type=ut,typeof o.type>`u`)throw new Error(`Actions may not have an undefined "type" property. Have you misspelled a constant?`)}};var te=class{constructor(){this.type=xi}};var ee=class{constructor(o){this.timestamp=o,this.type=Vn}};var ne=class{constructor(o){this.timestamp=o,this.type=qn}};var ie=class{constructor(o){this.timestamp=o,this.type=Gn}};var oe=class{constructor(){this.type=Zn}};var ae=class{constructor(o){this.id=o,this.type=Xn}};var re=class{constructor(o){this.index=o,this.type=Kn}};var se=class{constructor(o){this.actionId=o,this.type=Qn}};var ce=class{constructor(o){this.nextLiftedState=o,this.type=pe}};var le=class{constructor(o){this.status=o,this.type=Wn}};var de=class{constructor(o){this.status=o,this.type=Yn}};var Dt=new A$2(`@ngrx/store-devtools Options`);var Pn=new A$2(`@ngrx/store-devtools Initial Config`);function Jn(){return null}var ki=`NgRx Store DevTools`;function Ai(e){let o={maxAge:!1,monitor:Jn,actionSanitizer:void 0,stateSanitizer:void 0,actionCreators:void 0,name:ki,serialize:!1,logOnly:!1,autoPause:!1,trace:!1,traceLimit:75,features:{pause:!0,lock:!0,persist:!0,export:!0,import:`custom`,jump:!0,skip:!0,reorder:!0,dispatch:!0,test:!0},connectInZone:!1},t=typeof e==`function`?e():e,n=t.logOnly?{pause:!0,export:!0,test:!0}:!1,i=t.features||n||o.features;i.import===!0&&(i.import=`custom`);let a=Object.assign({},o,{features:i},t);if(a.maxAge&&a.maxAge<2)throw new Error(`Devtools 'maxAge' cannot be less than 2, got ${a.maxAge}`);return a}function Fn(e,o){return e.filter(t=>o.indexOf(t)<0)}function ti(e){let{computedStates:o,currentStateIndex:t}=e;if(t>=o.length){let{state:i}=o[o.length-1];return i}let{state:n}=o[t];return n}function pt(e){return new J(e,+Date.now())}function wi(e,o){return Object.keys(o).reduce((t,n)=>{let i=Number(n);return t[i]=ei(e,o[i],i),t},{})}function ei(e,o,t){return m(l({},o),{action:e(o.action,t)})}function Ci(e,o){return o.map((t,n)=>({state:ni(e,t.state,n),error:t.error}))}function ni(e,o,t){return e(o,t)}function ii(e){return e.predicate||e.actionsSafelist||e.actionsBlocklist}function Ei(e,o,t,n){let i=[],a={},r=[];return e.stagedActionIds.forEach((d,f)=>{let s=e.actionsById[d];s&&(f&&ue(e.computedStates[f],s,o,t,n)||(a[d]=s,i.push(d),r.push(e.computedStates[f])))}),m(l({},e),{stagedActionIds:i,actionsById:a,computedStates:r})}function ue(e,o,t,n,i){let a=t&&!t(e,o.action),r=n&&!o.action.type.match(n.map(f=>Ln(f)).join(`|`)),d=i&&o.action.type.match(i.map(f=>Ln(f)).join(`|`));return a||r||d}function Ln(e){return e.replace(/[.*+?^${}()|[\]\\]/g,`\\$&`)}function oi(e){return{ngZone:e?T$1(xe):null,connectInZone:e}}var Bt=(()=>{class e extends R$1{static{this.ɵfac=(()=>{let t;return function(i){return(t||(t=ey(e)))(i||e)}})()}static{this.ɵprov=ie$2({token:e,factory:e.ɵfac})}}return e})();var Rt={START:`START`,DISPATCH:`DISPATCH`,STOP:`STOP`,ACTION:`ACTION`};var me=new A$2(`@ngrx/store-devtools Redux Devtools Extension`);function Ii(e){return typeof e==`object`&&e!==null&&!(`type`in e)&&typeof e.selected==`number`&&Array.isArray(e.args)}function zn(e){let o=String(e),t=o.match(/^[^(]*\(([^)]*)\)/);if(!t){let n=o.match(/^\s*([^=\s(]+)\s*=>/);return n?[n[1]]:[]}return t[1].split(`,`).map(n=>n.replace(/^\s*\.{3}/,``).split(`=`)[0].trim()).filter(n=>n!==``)}function Ti(e){return Array.isArray(e)?e.map(o=>({name:o.type||o.name||`anonymous`,func:o,args:zn(o)})):Object.keys(e).map(o=>({name:o,func:e[o],args:zn(e[o])}))}var Hn=e=>e===``?void 0:(0,eval)(`(${e})`);var ai=(()=>{class e{constructor(t,n,i){this.config=n,this.dispatcher=i,this.zoneConfig=oi(this.config.connectInZone),this.devtoolsExtension=t,this.actionCreatorDescriptors=n.actionCreators?Ti(n.actionCreators):void 0,this.createActionStreams()}notify(t,n){if(this.devtoolsExtension)if(t.type===ut){if(n.isLocked||n.isPaused)return;let i=ti(n);if(ii(this.config)&&ue(i,t,this.config.predicate,this.config.actionsSafelist,this.config.actionsBlocklist))return;let a=this.config.stateSanitizer?ni(this.config.stateSanitizer,i,n.currentStateIndex):i,r=this.config.actionSanitizer?ei(this.config.actionSanitizer,t,n.nextActionId):t;this.sendToReduxDevtools(()=>this.extensionConnection.send(r,a))}else{let i=m(l({},n),{stagedActionIds:n.stagedActionIds,actionsById:this.config.actionSanitizer?wi(this.config.actionSanitizer,n.actionsById):n.actionsById,computedStates:this.config.stateSanitizer?Ci(this.config.stateSanitizer,n.computedStates):n.computedStates});this.sendToReduxDevtools(()=>this.devtoolsExtension.send(null,i,this.getExtensionConfig(this.config)))}}createChangesObservable(){return this.devtoolsExtension?new _(t=>{let n=this.zoneConfig.connectInZone?this.zoneConfig.ngZone.runOutsideAngular(()=>this.devtoolsExtension.connect(this.getExtensionConfig(this.config))):this.devtoolsExtension.connect(this.getExtensionConfig(this.config));return this.extensionConnection=n,n.init(),n.subscribe(i=>t.next(i)),n.unsubscribe}):$e}createActionStreams(){let t=this.createChangesObservable().pipe(fs()),n=t.pipe(Bn(s=>s.type===Rt.START)),i=t.pipe(Bn(s=>s.type===Rt.STOP)),a=t.pipe(Bn(s=>s.type===Rt.DISPATCH),ce$1(s=>this.unwrapAction(s.payload)),vg(s=>s.type===pe?this.dispatcher.pipe(Bn(y=>y.type===rt$3),rg(1e3),Ig(1e3),ce$1(()=>s),ls(()=>ss(s)),us(1)):ss(s))),d=t.pipe(Bn(s=>s.type===Rt.ACTION),ce$1(s=>this.unwrapAction(s.payload))).pipe(Og(i)),f=a.pipe(Og(i));this.start$=n.pipe(Og(i)),this.actions$=this.start$.pipe(ql(()=>d)),this.liftedActions$=this.start$.pipe(ql(()=>f))}unwrapAction(t){if(typeof t==`string`)return(0,eval)(`(${t})`);if(this.actionCreatorDescriptors&&Ii(t)){let n=this.actionCreatorDescriptors[t.selected];if(n){let i=t.args.map(Hn);if(t.rest){let a=Hn(t.rest);Array.isArray(a)&&i.push(...a)}return n.func(...i)}}return t}getExtensionConfig(t){let n={name:t.name,features:t.features,serialize:t.serialize,autoPause:t.autoPause??!1,trace:t.trace??!1,traceLimit:t.traceLimit??75};return t.maxAge!==!1&&(n.maxAge=t.maxAge),this.actionCreatorDescriptors&&(n.actionCreators=this.actionCreatorDescriptors),n}sendToReduxDevtools(t){try{t()}catch(n){console.warn(`@ngrx/store-devtools: something went wrong inside the redux devtools`,n)}}static{this.ɵfac=function(n){return new(n||e)(Se(me),Se(Dt),Se(Bt))}}static{this.ɵprov=ie$2({token:e,factory:e.ɵfac})}}return e})();var Nt={type:Te$1};var Ri={type:`@ngrx/store-devtools/recompute`};function ri(e,o,t,n,i){if(n)return{state:t,error:`Interrupted by an error up the chain`};let a=t,r;try{a=e(t,o)}catch(d){r=d.toString(),i.handleError(d)}return{state:a,error:r}}function Ot(e,o,t,n,i,a,r,d,f){if(o>=e.length&&e.length===a.length)return e;let s=e.slice(0,o),y=a.length-(f?1:0);for(let c=o;c<y;c++){let u=a[c],k=i[u].action,p=s[c-1],m=p?p.state:n,R=p?p.error:void 0,O=r.indexOf(u)>-1?p:ri(t,k,m,R,d);s.push(O)}return f&&s.push(e[e.length-1]),s}function Oi(e,o){return{monitorState:o(void 0,{}),nextActionId:1,actionsById:{0:pt(Nt)},stagedActionIds:[0],skippedActionIds:[],committedState:e,currentStateIndex:0,computedStates:[],isLocked:!1,isPaused:!1}}function Ni(e,o,t,n,i={}){return a=>(r,d)=>{let{monitorState:f,actionsById:s,nextActionId:y,stagedActionIds:c,skippedActionIds:u,committedState:k,currentStateIndex:p,computedStates:m$1,isLocked:R,isPaused:v}=r||o;r||(s=Object.create(s));function O(b){let g=b,P=c.slice(1,g+1);for(let I=0;I<P.length;I++)if(m$1[I+1].error){g=I,P=c.slice(1,g+1);break}else delete s[P[I]];u=u.filter(I=>P.indexOf(I)===-1),c=[0,...c.slice(g+1)],k=m$1[g].state,m$1=m$1.slice(g),p=p>g?p-g:0}function N(){s={0:pt(Nt)},y=1,c=[0],u=[],k=m$1[p].state,p=0,m$1=[]}let h=0;switch(d.type){case Wn:R=d.status,h=Infinity;break;case Yn:v=d.status,v?(c=[...c,y],s[y]=new J({type:`@ngrx/devtools/pause`},+Date.now()),y++,h=c.length-1,m$1=m$1.concat(m$1[m$1.length-1]),p===c.length-2&&p++,h=Infinity):N();break;case Vn:s={0:pt(Nt)},y=1,c=[0],u=[],k=e,p=0,m$1=[];break;case Gn:N();break;case qn:s={0:pt(Nt)},y=1,c=[0],u=[],p=0,m$1=[];break;case Xn:{let{id:b}=d;u.indexOf(b)===-1?u=[b,...u]:u=u.filter(P=>P!==b),h=c.indexOf(b);break}case Si:{let{start:b,end:g,active:P}=d,I=[];for(let $t=b;$t<g;$t++)I.push($t);P?u=Fn(u,I):u=[...u,...I],h=c.indexOf(b);break}case Kn:p=d.index,h=Infinity;break;case Qn:{let b=c.indexOf(d.actionId);b!==-1&&(p=b),h=Infinity;break}case Zn:c=Fn(c,u),u=[],p=Math.min(p,c.length-1);break;case ut:{if(R)return r||o;if(v||r&&ue(r.computedStates[p],d,i.predicate,i.actionsSafelist,i.actionsBlocklist)){let g=m$1[m$1.length-1];m$1=[...m$1.slice(0,-1),ri(a,d.action,g.state,g.error,t)],h=Infinity;break}i.maxAge&&c.length===i.maxAge&&O(1),p===c.length-1&&p++;let b=y++;s[b]=d,c=[...c,b],h=c.length-1;break}case pe:({monitorState:f,actionsById:s,nextActionId:y,stagedActionIds:c,skippedActionIds:u,committedState:k,currentStateIndex:p,computedStates:m$1,isLocked:R,isPaused:v}=d.nextLiftedState);break;case Te$1:h=0,i.maxAge&&c.length>i.maxAge&&(m$1=Ot(m$1,h,a,k,s,c,u,t,v),O(c.length-i.maxAge),h=Infinity);break;case rt$3:if(m$1.filter(g=>g.error).length>0)h=0,i.maxAge&&c.length>i.maxAge&&(m$1=Ot(m$1,h,a,k,s,c,u,t,v),O(c.length-i.maxAge),h=Infinity);else{if(!v&&!R){p===c.length-1&&p++;let g=y++;s[g]=new J(d,+Date.now()),c=[...c,g],h=c.length-1,m$1=Ot(m$1,h,a,k,s,c,u,t,v)}m$1=m$1.map(g=>m(l({},g),{state:a(g.state,Ri)})),p=c.length-1,i.maxAge&&c.length>i.maxAge&&O(c.length-i.maxAge),h=Infinity}break;default:h=Infinity;break}return m$1=Ot(m$1,h,a,k,s,c,u,t,v),f=n(f,d),{monitorState:f,actionsById:s,nextActionId:y,stagedActionIds:c,skippedActionIds:u,committedState:k,currentStateIndex:p,computedStates:m$1,isLocked:R,isPaused:v}}}var $n=(()=>{class e{constructor(t,n,i,a,r,d,f,s){let y=Oi(f,s.monitor),c=Ni(f,y,d,s.monitor,s),u=mg(mg(n.asObservable().pipe(Rg(1)),a.actions$).pipe(ce$1(pt)),t,a.liftedActions$).pipe(jn$1(Gh)),k=i.pipe(ce$1(c)),p=oi(s.connectInZone),m=new Ln$1(1);this.liftedStateSubscription=u.pipe(Pg(k),Un(p),Ag(({state:O},[N,h])=>{let b=h(O,N);return N.type!==ut&&ii(s)&&(b=Ei(b,s.predicate,s.actionsSafelist,s.actionsBlocklist)),a.notify(N,b),{state:b,action:N}},{state:y,action:null})).subscribe(({state:O,action:N})=>{if(m.next(O),N.type===ut){let h=N.action;r.next(h)}}),this.extensionStartSubscription=a.start$.pipe(Un(p)).subscribe(()=>{this.refresh()});let R=m.asObservable(),v=R.pipe(ce$1(ti));Object.defineProperty(v,"state",{value:ye(v,{manualCleanup:!0,requireSync:!0})}),this.dispatcher=t,this.liftedState=R,this.state=v}ngOnDestroy(){this.liftedStateSubscription.unsubscribe(),this.extensionStartSubscription.unsubscribe()}dispatch(t){this.dispatcher.next(t)}next(t){this.dispatcher.next(t)}error(t){}complete(){}performAction(t){this.dispatch(new J(t,+Date.now()))}refresh(){this.dispatch(new te)}reset(){this.dispatch(new ee(+Date.now()))}rollback(){this.dispatch(new ne(+Date.now()))}commit(){this.dispatch(new ie(+Date.now()))}sweep(){this.dispatch(new oe)}toggleAction(t){this.dispatch(new ae(t))}jumpToAction(t){this.dispatch(new se(t))}jumpToState(t){this.dispatch(new re(t))}importState(t){this.dispatch(new ce(t))}lockChanges(t){this.dispatch(new le(t))}pauseRecording(t){this.dispatch(new de(t))}static{this.ɵfac=function(n){return new(n||e)(Se(Bt),Se(R$1),Se(E),Se(ai),Se(J$4),Se(it$2),Se(W$3),Se(Dt))}}static{this.ɵprov=ie$2({token:e,factory:e.ɵfac})}}return e})();function Un({ngZone:e,connectInZone:o}){return t=>o?new _(n=>t.subscribe({next:i=>e.run(()=>n.next(i)),error:i=>e.run(()=>n.error(i)),complete:()=>e.run(()=>n.complete())})):t}var Di=new A$2(`@ngrx/store-devtools Is Devtools Extension or Monitor Present`);function Bi(e,o){return!!e||o.monitor!==Jn}function ji(){let e=`__REDUX_DEVTOOLS_EXTENSION__`;return typeof window==`object`&&typeof window[e]<`u`?window[e]:null}function Pi(e){return e.state}function si(e={}){return _o([ai,Bt,$n,{provide:Pn,useValue:e},{provide:Di,deps:[me,Dt],useFactory:Bi},{provide:me,useFactory:ji},{provide:Dt,deps:[Pn],useFactory:Ai},{provide:D$1,deps:[$n],useFactory:Pi},{provide:w,useExisting:Bt}])}var ci=[{path:`auth`,loadChildren:()=>import(`./chunk-B49D3MwD.js`).then(e=>e.authRoutes)},{path:`projects`,canActivate:[p],loadChildren:()=>import(`./chunk-BJ0kXt9x.js`).then(e=>e.projectsRoutes)},{path:`tasks`,canActivate:[p],loadChildren:()=>import(`./chunk-DQBb9szm.js`).then(e=>e.tasksRoutes)},{path:`team`,canActivate:[p],loadChildren:()=>import(`./chunk-gyqqa4rG.js`).then(e=>e.teamRoutes)},{path:``,pathMatch:`full`,canActivate:[p],loadComponent:()=>import(`./chunk-xy3P2z0a.js`).then(e=>e.HomeComponent)}];var $=class e{nextId=0;messages=$o([]);notifyError(o){this.push(o,`error`)}notifyInfo(o){this.push(o,`info`)}dismiss(o){this.messages.update(t=>t.filter(n=>n.id!==o))}push(o,t){let n=this.nextId++;this.messages.update(i=>[...i,{id:n,text:o,level:t}]),setTimeout(()=>this.dismiss(n),6e3)}static ɵfac=function(t){return new(t||e)};static ɵprov=ie$2({token:e,factory:e.ɵfac,providedIn:`root`})};var jt=null;var li=(e,o)=>{let t=T$1($),n$1=T$1(n),i=T$1(X$2);return o(e).pipe(ls(a=>{if(!(a instanceof Ne))return as(()=>a);if(a.status===401&&!e.url.includes(`/api/auth/login`)&&!e.url.includes(`/api/auth/refresh`))return Fi(e,o,n$1,i);if(a.status!==401){let r=a.error&&typeof a.error==`object`&&`title`in a.error?String(a.error.title):`Request failed.`;t.notifyError(r)}return as(()=>a)}))};function Fi(e,o,t,n){return jt||(jt=t.refresh().pipe(Ul(1),$l(()=>{jt=null}))),jt.pipe(ql(i=>{n.dispatch(In.tokenRefreshed({accessToken:i.accessToken,expiresAt:i.expiresAt,user:{id:i.user.id,fullName:i.user.fullName,email:i.user.email,role:i.user.role}}));return o(e.clone({setHeaders:{Authorization:`Bearer ${i.accessToken}`}}))}),ls(i=>(n.dispatch(In.logoutRequested()),as(()=>i))))}var di=(e,o)=>{let n=T$1(X$2).selectSignal(f.selectAccessToken)(),i=e;return n&&(i=i.clone({setHeaders:{Authorization:`Bearer ${n}`}})),i.url.startsWith(`/api/auth/`)&&(i=i.clone({withCredentials:!0})),o(i)};var Pt=class e{notifications=T$1($);handleError(o){console.error(o),this.notifications.notifyError(`Something went wrong. Please try again.`)}static ɵfac=function(t){return new(t||e)};static ɵprov=ie$2({token:e,factory:e.ɵfac})};var Ft=class e{actions$=T$1(bt$1);authService=T$1(n);router=T$1(Ue$1);login$=Ct(()=>this.actions$.pipe(Dt$1(In.loginSubmitted),ql(({email:o,password:t})=>this.authService.login({email:o,password:t}).pipe(ce$1(n=>In.loginSuccess({accessToken:n.accessToken,expiresAt:n.expiresAt,user:{id:n.user.id,fullName:n.user.fullName,email:n.user.email,role:n.user.role}})),ls(n=>ss(In.loginFailure({error:n.error?.title??`Invalid credentials`})))))));logout$=Ct(()=>this.actions$.pipe(Dt$1(In.logoutRequested),ql(()=>this.authService.logout().pipe(ce$1(()=>In.logoutCompleted()),ls(()=>ss(In.logoutCompleted()))))));logoutCompleted$=Ct(()=>this.actions$.pipe(Dt$1(In.logoutCompleted),Wl(()=>{this.router.navigateByUrl(`/auth/login`)})),{dispatch:!1});static ɵfac=function(t){return new(t||e)};static ɵprov=ie$2({token:e,factory:e.ɵfac})};var mi={providers:[gm(),jn(),_c(ci),$a$1(za([di,li]),Ha({cookieName:`XSRF-TOKEN`,headerName:`X-XSRF-TOKEN`})),vn(),gn(f),wt$1(Ft),si({maxAge:25,logOnly:!n1()}),{provide:it$2,useClass:Pt},{provide:Qe,useValue:{subscriptSizing:`dynamic`}}]};function Li(e,o){if(e&1){let t=JE();Ei$1(0,`div`,1)(1,`button`,2),zp(`click`,function(){bu(t);return _u(iD().action())}),PD(2),Hc()()}if(e&2){let t=iD();Bv(2),Wc(` `,t.data.action,` `)}}var zi=[`label`];function Hi(e,o){}var $i=Math.pow(2,31)-1;var ft=class{_overlayRef;instance;containerInstance;_afterDismissed=new z$1;_afterOpened=new z$1;_onAction=new z$1;_durationTimeoutId;_dismissedByAction=!1;constructor(o,t){this._overlayRef=t,this.containerInstance=o,o._onExit.subscribe(()=>this._finishDismiss())}dismiss(){this._afterDismissed.closed||this.containerInstance.exit(),clearTimeout(this._durationTimeoutId)}dismissWithAction(){this._onAction.closed||(this._dismissedByAction=!0,this._onAction.next(),this._onAction.complete(),this.dismiss()),clearTimeout(this._durationTimeoutId)}closeWithAction(){this.dismissWithAction()}_dismissAfter(o){this._durationTimeoutId=setTimeout(()=>this.dismiss(),Math.min(o,$i))}_open(){this._afterOpened.closed||(this._afterOpened.next(),this._afterOpened.complete())}_finishDismiss(){this._overlayRef.dispose(),this._onAction.closed||this._onAction.complete(),this._afterDismissed.next({dismissedByAction:this._dismissedByAction}),this._afterDismissed.complete(),this._dismissedByAction=!1}afterDismissed(){return this._afterDismissed}afterOpened(){return this.containerInstance._onEnter}onAction(){return this._onAction}};var pi=new A$2(`MatSnackBarData`);var tt=class{politeness=`polite`;announcementMessage=``;viewContainerRef;duration=0;panelClass;direction;data=null;horizontalPosition=`center`;verticalPosition=`bottom`};var Ui=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵdir=wE({type:e,selectors:[[``,`matSnackBarLabel`,``]],hostAttrs:[1,`mat-mdc-snack-bar-label`,`mdc-snackbar__label`]})}return e})();var Vi=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵdir=wE({type:e,selectors:[[``,`matSnackBarActions`,``]],hostAttrs:[1,`mat-mdc-snack-bar-actions`,`mdc-snackbar__actions`]})}return e})();var qi=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵdir=wE({type:e,selectors:[[``,`matSnackBarAction`,``]],hostAttrs:[1,`mat-mdc-snack-bar-action`,`mdc-snackbar__action`]})}return e})();var ui=(()=>{class e{snackBarRef=T$1(ft);data=T$1(pi);action(){this.snackBarRef.dismissWithAction()}get hasAction(){return!!this.data.action}static ɵfac=function(n){return new(n||e)};static ɵcmp=yE({type:e,selectors:[[`simple-snack-bar`]],hostAttrs:[1,`mat-mdc-simple-snack-bar`],exportAs:[`matSnackBar`],decls:3,vars:2,consts:[[`matSnackBarLabel`,``],[`matSnackBarActions`,``],[`matButton`,``,`matSnackBarAction`,``,3,`click`]],template:function(n,i){n&1&&(Ei$1(0,`div`,0),PD(1),Hc(),$E(2,Li,3,1,`div`,1)),n&2&&(Bv(),Wc(` `,i.data.message,`
`),Bv(),UE(i.hasAction?2:-1))},dependencies:[tr,Ui,Vi,qi],styles:[`.mat-mdc-simple-snack-bar {
  display: flex;
}
.mat-mdc-simple-snack-bar .mat-mdc-snack-bar-label {
  max-height: 50vh;
  overflow: auto;
}
`],encapsulation:2})}return e})();var fe=`_mat-snack-bar-enter`;var he=`_mat-snack-bar-exit`;var Gi=(()=>{class e extends j$1{_ngZone=T$1(xe);_elementRef=T$1(Er);_changeDetectorRef=T$1(YF);_platform=T$1(p$1);_animationsDisabled=z$2();snackBarConfig=T$1(tt);_document=T$1(or);_trackedModals=new Set;_enterFallback;_exitFallback;_injector=T$1(ge);_announceDelay=150;_announceTimeoutId;_destroyed=!1;_portalOutlet;_onAnnounce=new z$1;_onExit=new z$1;_onEnter=new z$1;_animationState=`void`;_live;_label;_role;_liveElementId=T$1(Pt$1).getId(`mat-snack-bar-container-live-`);constructor(){super();let t=this.snackBarConfig;t.politeness===`assertive`&&!t.announcementMessage?this._live=`assertive`:t.politeness===`off`?this._live=`off`:this._live=`polite`,this._platform.FIREFOX&&(this._live===`polite`&&(this._role=`status`),this._live===`assertive`&&(this._role=`alert`))}attachComponentPortal(t){this._assertNotAttached();let n=this._portalOutlet.attachComponentPortal(t);return this._afterPortalAttached(),n}attachTemplatePortal(t){this._assertNotAttached();let n=this._portalOutlet.attachTemplatePortal(t);return this._afterPortalAttached(),n}attachDomPortal=t=>{this._assertNotAttached();let n=this._portalOutlet.attachDomPortal(t);return this._afterPortalAttached(),n};onAnimationEnd(t){t===he?this._completeExit():t===fe&&(clearTimeout(this._enterFallback),this._ngZone.run(()=>{this._onEnter.next(),this._onEnter.complete()}))}enter(){this._destroyed||(this._animationState=`visible`,this._changeDetectorRef.markForCheck(),this._changeDetectorRef.detectChanges(),this._screenReaderAnnounce(),this._animationsDisabled?mv(()=>{this._ngZone.run(()=>queueMicrotask(()=>this.onAnimationEnd(fe)))},{injector:this._injector}):(clearTimeout(this._enterFallback),this._enterFallback=setTimeout(()=>{this._elementRef.nativeElement.classList.add(`mat-snack-bar-fallback-visible`),this.onAnimationEnd(fe)},200)))}exit(){return this._destroyed?ss(void 0):(this._ngZone.run(()=>{this._animationState=`hidden`,this._changeDetectorRef.markForCheck(),this._elementRef.nativeElement.setAttribute(`mat-exit`,``),clearTimeout(this._announceTimeoutId),this._animationsDisabled?mv(()=>{this._ngZone.run(()=>queueMicrotask(()=>this.onAnimationEnd(he)))},{injector:this._injector}):(clearTimeout(this._exitFallback),this._exitFallback=setTimeout(()=>this.onAnimationEnd(he),200))}),this._onExit)}ngOnDestroy(){this._destroyed=!0,this._clearFromModals(),this._completeExit()}_completeExit(){clearTimeout(this._exitFallback),queueMicrotask(()=>{this._onExit.next(),this._onExit.complete()})}_afterPortalAttached(){let t=this._elementRef.nativeElement,n=this.snackBarConfig.panelClass;n&&(Array.isArray(n)?n.forEach(r=>t.classList.add(r)):t.classList.add(n)),this._exposeToModals();let i=this._label.nativeElement,a=`mdc-snackbar__label`;i.classList.toggle(a,!i.querySelector(`.${a}`))}_exposeToModals(){let t=this._liveElementId,n=this._document.querySelectorAll(`body > .cdk-overlay-container [aria-modal="true"]`);for(let i=0;i<n.length;i++){let a=n[i],r=a.getAttribute(`aria-owns`);this._trackedModals.add(a),r?r.indexOf(t)===-1&&a.setAttribute(`aria-owns`,r+` `+t):a.setAttribute(`aria-owns`,t)}}_clearFromModals(){this._trackedModals.forEach(t=>{let n=t.getAttribute(`aria-owns`);if(n){let i=n.replace(this._liveElementId,``).trim();i.length>0?t.setAttribute(`aria-owns`,i):t.removeAttribute(`aria-owns`)}}),this._trackedModals.clear()}_assertNotAttached(){this._portalOutlet.hasAttached()}_screenReaderAnnounce(){this._announceTimeoutId||this._ngZone.runOutsideAngular(()=>{this._announceTimeoutId=setTimeout(()=>{if(this._destroyed)return;let t=this._elementRef.nativeElement,n=t.querySelector(`[aria-hidden]`),i=t.querySelector(`[aria-live]`);if(n&&i){let a=null;this._platform.isBrowser&&document.activeElement instanceof HTMLElement&&n.contains(document.activeElement)&&(a=document.activeElement),n.removeAttribute(`aria-hidden`),i.appendChild(n),a?.focus(),this._onAnnounce.next(),this._onAnnounce.complete()}},this._announceDelay)})}static ɵfac=function(n){return new(n||e)};static ɵcmp=yE({type:e,selectors:[[`mat-snack-bar-container`]],viewQuery:function(n,i){if(n&1&&Kp(pe$1,7)(zi,7),n&2){let a;uD(a=dD())&&(i._portalOutlet=a.first),uD(a=dD())&&(i._label=a.first)}},hostAttrs:[1,`mdc-snackbar`,`mat-mdc-snack-bar-container`],hostVars:6,hostBindings:function(n,i){n&1&&zp(`animationend`,function(r){return i.onAnimationEnd(r.animationName)})(`animationcancel`,function(r){return i.onAnimationEnd(r.animationName)}),n&2&&nh(`mat-snack-bar-container-enter`,i._animationState===`visible`)(`mat-snack-bar-container-exit`,i._animationState===`hidden`)(`mat-snack-bar-container-animations-enabled`,!i._animationsDisabled)},features:[Rp],decls:6,vars:3,consts:[[`label`,``],[1,`mdc-snackbar__surface`,`mat-mdc-snackbar-surface`],[1,`mat-mdc-snack-bar-label`],[`aria-hidden`,`true`],[`cdkPortalOutlet`,``]],template:function(n,i){n&1&&(Ei$1(0,`div`,1)(1,`div`,2,0)(3,`div`,3),Op(4,Hi,0,0,`ng-template`,4),Hc(),Bp(5,`div`),Hc()()),n&2&&(Bv(5),Vp(`aria-live`,i._live)(`role`,i._role)(`id`,i._liveElementId))},dependencies:[pe$1],styles:[`@keyframes _mat-snack-bar-enter {
  from {
    transform: scale(0.8);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}
@keyframes _mat-snack-bar-exit {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}
.mat-mdc-snack-bar-container {
  display: flex;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
  -webkit-tap-highlight-color: rgba(0, 0, 0, 0);
  margin: 8px;
}
.mat-mdc-snack-bar-handset .mat-mdc-snack-bar-container {
  width: 100vw;
}

.mat-snack-bar-container-animations-enabled {
  opacity: 0;
}
.mat-snack-bar-container-animations-enabled.mat-snack-bar-fallback-visible {
  opacity: 1;
}
.mat-snack-bar-container-animations-enabled.mat-snack-bar-container-enter {
  animation: _mat-snack-bar-enter 150ms cubic-bezier(0, 0, 0.2, 1) forwards;
}
.mat-snack-bar-container-animations-enabled.mat-snack-bar-container-exit {
  animation: _mat-snack-bar-exit 75ms cubic-bezier(0.4, 0, 1, 1) forwards;
}

.mat-mdc-snackbar-surface {
  box-shadow: 0px 3px 5px -1px rgba(0, 0, 0, 0.2), 0px 6px 10px 0px rgba(0, 0, 0, 0.14), 0px 1px 18px 0px rgba(0, 0, 0, 0.12);
  display: flex;
  align-items: center;
  justify-content: flex-start;
  box-sizing: border-box;
  padding-left: 0;
  padding-right: 8px;
}
[dir=rtl] .mat-mdc-snackbar-surface {
  padding-right: 0;
  padding-left: 8px;
}
.mat-mdc-snack-bar-container .mat-mdc-snackbar-surface {
  min-width: 344px;
  max-width: 672px;
}
.mat-mdc-snack-bar-handset .mat-mdc-snackbar-surface {
  width: 100%;
  min-width: 0;
}
@media (forced-colors: active) {
  .mat-mdc-snackbar-surface {
    outline: solid 1px;
  }
}
.mat-mdc-snack-bar-container .mat-mdc-snackbar-surface {
  color: var(--%NS%mat-snack-bar-supporting-text-color, var(--%NS%mat-sys-inverse-on-surface));
  border-radius: var(--%NS%mat-snack-bar-container-shape, var(--%NS%mat-sys-corner-extra-small));
  background-color: var(--%NS%mat-snack-bar-container-color, var(--%NS%mat-sys-inverse-surface));
}

.mdc-snackbar__label {
  width: 100%;
  flex-grow: 1;
  box-sizing: border-box;
  margin: 0;
  padding: 14px 8px 14px 16px;
}
[dir=rtl] .mdc-snackbar__label {
  padding-left: 8px;
  padding-right: 16px;
}
.mat-mdc-snack-bar-container .mdc-snackbar__label {
  font-family: var(--%NS%mat-snack-bar-supporting-text-font, var(--%NS%mat-sys-body-medium-font));
  font-size: var(--%NS%mat-snack-bar-supporting-text-size, var(--%NS%mat-sys-body-medium-size));
  font-weight: var(--%NS%mat-snack-bar-supporting-text-weight, var(--%NS%mat-sys-body-medium-weight));
  line-height: var(--%NS%mat-snack-bar-supporting-text-line-height, var(--%NS%mat-sys-body-medium-line-height));
}

.mat-mdc-snack-bar-actions {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  box-sizing: border-box;
}

.mat-mdc-snack-bar-handset,
.mat-mdc-snack-bar-container,
.mat-mdc-snack-bar-label {
  flex: 1 1 auto;
}

.mat-mdc-snack-bar-container .mat-mdc-button.mat-mdc-snack-bar-action:not(:disabled).mat-unthemed {
  color: var(--%NS%mat-snack-bar-button-color, var(--%NS%mat-sys-inverse-primary));
}
.mat-mdc-snack-bar-container .mat-mdc-button.mat-mdc-snack-bar-action:not(:disabled) {
  --%NS%mat-button-text-state-layer-color: currentColor;
  --%NS%mat-button-text-ripple-color: currentColor;
}
.mat-mdc-snack-bar-container .mat-mdc-button.mat-mdc-snack-bar-action:not(:disabled) .mat-ripple-element {
  opacity: 0.1;
}
`],encapsulation:2,changeDetection:1})}return e})();var Zi=new A$2(`mat-snack-bar-default-options`,{providedIn:`root`,factory:()=>new tt});var Xi=(()=>{class e{_live=T$1(In$1);_injector=T$1(ge);_breakpointObserver=T$1(Ot$1);_parentSnackBar=T$1(e,{optional:!0,skipSelf:!0});_defaultConfig=T$1(Zi);_animationsDisabled=z$2();_snackBarRefAtThisLevel=null;simpleSnackBarComponent=ui;snackBarContainerComponent=Gi;handsetCssClass=`mat-mdc-snack-bar-handset`;get _openedSnackBarRef(){let t=this._parentSnackBar;return t?t._openedSnackBarRef:this._snackBarRefAtThisLevel}set _openedSnackBarRef(t){this._parentSnackBar?this._parentSnackBar._openedSnackBarRef=t:this._snackBarRefAtThisLevel=t}openFromComponent(t,n){return this._attach(t,n)}openFromTemplate(t,n){return this._attach(t,n)}open(t,n=``,i){let a=l(l({},this._defaultConfig),i);return a.data={message:t,action:n},a.announcementMessage===t&&(a.announcementMessage=void 0),this.openFromComponent(this.simpleSnackBarComponent,a)}dismiss(){this._openedSnackBarRef&&this._openedSnackBarRef.dismiss()}ngOnDestroy(){this._snackBarRefAtThisLevel&&this._snackBarRefAtThisLevel.dismiss()}_attachSnackBarContainer(t,n){let i=n&&n.viewContainerRef&&n.viewContainerRef.injector,a=ge.create({parent:i||this._injector,providers:[{provide:tt,useValue:n}]}),r=new at$1(this.snackBarContainerComponent,n.viewContainerRef,a),d=t.attach(r);return d.instance.snackBarConfig=n,d.instance}_attach(t,n){let i=l(l(l({},new tt),this._defaultConfig),n),a=this._createOverlay(i),r=this._attachSnackBarContainer(a,i),d=new ft(r,a);if(t instanceof hr){let f=new M(t,null,{$implicit:i.data,snackBarRef:d});d.instance=r.attachTemplatePortal(f)}else{let s=new at$1(t,void 0,this._createInjector(i,d));d.instance=r.attachComponentPortal(s).instance}return this._breakpointObserver.observe(ei$1.HandsetPortrait).pipe(Og(a.detachments())).subscribe(f=>{a.overlayElement.classList.toggle(this.handsetCssClass,f.matches)}),i.announcementMessage&&r._onAnnounce.subscribe(()=>{this._live.announce(i.announcementMessage,i.politeness)}),this._animateSnackBar(d,i),this._openedSnackBarRef=d,this._openedSnackBarRef}_animateSnackBar(t,n){t.afterDismissed().subscribe(()=>{this._openedSnackBarRef==t&&(this._openedSnackBarRef=null),n.announcementMessage&&this._live.clear()}),n.duration&&n.duration>0&&t.afterOpened().subscribe(()=>t._dismissAfter(n.duration)),this._openedSnackBarRef?(this._openedSnackBarRef.afterDismissed().subscribe(()=>{t.containerInstance.enter()}),this._openedSnackBarRef.dismiss()):t.containerInstance.enter()}_createOverlay(t){let n=new B;n.direction=t.direction;let i=$t(this._injector),a=t.direction===`rtl`,r=t.horizontalPosition===`left`||t.horizontalPosition===`start`&&!a||t.horizontalPosition===`end`&&a,d=!r&&t.horizontalPosition!==`center`;return r?i.left(`0`):d?i.right(`0`):i.centerHorizontally(),t.verticalPosition===`top`?i.top(`0`):i.bottom(`0`),n.positionStrategy=i,n.disableAnimations=this._animationsDisabled,gt$1(this._injector,n)}_createInjector(t,n){let i=t&&t.viewContainerRef&&t.viewContainerRef.injector;return ge.create({parent:i||this._injector,providers:[{provide:ft,useValue:n},{provide:pi,useValue:t.data}]})}static ɵfac=function(n){return new(n||e)};static ɵprov=Ir({token:e,factory:e.ɵfac})}return e})();var fi=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵmod=IE({type:e});static ɵinj=Jl({providers:[Xi],imports:[re$1,Vt,er,ui,yt$1]})}return e})();var Ki=(e,o)=>o.id;function Qi(e,o){if(e&1&&(Bc(0,`div`,1),PD(1),$c()),e&2){let t=o.$implicit;nh(`notification--error`,t.level===`error`),Bv(),Wc(` `,t.text,` `)}}var Lt=class e{notifications=T$1($);static ɵfac=function(t){return new(t||e)};static ɵcmp=yE({type:e,selectors:[[`app-notification`]],decls:2,vars:0,consts:[[1,`notification`,3,`notification--error`],[1,`notification`]],template:function(t,n){t&1&&WE(0,Qi,2,3,`div`,0,Ki),t&2&&GE(n.notifications.messages())},dependencies:[fi],styles:[`.notification[_ngcontent-%COMP%]{position:fixed;bottom:16px;right:16px;padding:12px 16px;border-radius:4px;background:#323232;color:#fff;margin-top:8px;z-index:1000}.notification--error[_ngcontent-%COMP%]{background:#b3261e}`]})};var Wi=[`*`,[[`mat-toolbar-row`]]];var Yi=[`*`,`mat-toolbar-row`];var Ji=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵdir=wE({type:e,selectors:[[`mat-toolbar-row`]],hostAttrs:[1,`mat-toolbar-row`],exportAs:[`matToolbarRow`]})}return e})();var gi=(()=>{class e{_elementRef=T$1(Er);_platform=T$1(p$1);_document=T$1(or);color;_toolbarRows;ngAfterViewInit(){this._platform.isBrowser&&(this._checkToolbarMixedModes(),this._toolbarRows.changes.subscribe(()=>this._checkToolbarMixedModes()))}_checkToolbarMixedModes(){this._toolbarRows.length}static ɵfac=function(n){return new(n||e)};static ɵcmp=yE({type:e,selectors:[[`mat-toolbar`]],contentQueries:function(n,i,a){if(n&1&&Yp(a,Ji,5),n&2){let r;uD(r=dD())&&(i._toolbarRows=r)}},hostAttrs:[1,`mat-toolbar`],hostVars:6,hostBindings:function(n,i){n&2&&(TD(i.color?`mat-`+i.color:``),nh(`mat-toolbar-multiple-rows`,i._toolbarRows.length>0)(`mat-toolbar-single-row`,i._toolbarRows.length===0))},inputs:{color:`color`},exportAs:[`matToolbar`],ngContentSelectors:Yi,decls:2,vars:0,template:function(n,i){n&1&&(aD(Wi),cD(0),cD(1,1))},styles:[`.mat-toolbar {
  background: var(--%NS%mat-toolbar-container-background-color, var(--%NS%mat-sys-surface));
  color: var(--%NS%mat-toolbar-container-text-color, var(--%NS%mat-sys-on-surface));
}
.mat-toolbar, .mat-toolbar h1, .mat-toolbar h2, .mat-toolbar h3, .mat-toolbar h4, .mat-toolbar h5, .mat-toolbar h6 {
  font-family: var(--%NS%mat-toolbar-title-text-font, var(--%NS%mat-sys-title-large-font));
  font-size: var(--%NS%mat-toolbar-title-text-size, var(--%NS%mat-sys-title-large-size));
  line-height: var(--%NS%mat-toolbar-title-text-line-height, var(--%NS%mat-sys-title-large-line-height));
  font-weight: var(--%NS%mat-toolbar-title-text-weight, var(--%NS%mat-sys-title-large-weight));
  letter-spacing: var(--%NS%mat-toolbar-title-text-tracking, var(--%NS%mat-sys-title-large-tracking));
  margin: 0;
}
@media (forced-colors: active) {
  .mat-toolbar {
    outline: solid 1px;
  }
}
.mat-toolbar .mat-form-field-underline,
.mat-toolbar .mat-form-field-ripple,
.mat-toolbar .mat-focused .mat-form-field-ripple {
  background-color: currentColor;
}
.mat-toolbar .mat-form-field-label,
.mat-toolbar .mat-focused .mat-form-field-label,
.mat-toolbar .mat-select-value,
.mat-toolbar .mat-select-arrow,
.mat-toolbar .mat-form-field.mat-focused .mat-select-arrow {
  color: inherit;
}
.mat-toolbar .mat-input-element {
  caret-color: currentColor;
}
.mat-toolbar .mat-mdc-button-base.mat-mdc-button-base.mat-unthemed {
  --%NS%mat-button-text-label-text-color: var(--%NS%mat-toolbar-container-text-color, var(--%NS%mat-sys-on-surface));
  --%NS%mat-button-outlined-label-text-color: var(--%NS%mat-toolbar-container-text-color, var(--%NS%mat-sys-on-surface));
}

.mat-toolbar-row, .mat-toolbar-single-row {
  display: flex;
  box-sizing: border-box;
  padding: 0 16px;
  width: 100%;
  flex-direction: row;
  align-items: center;
  white-space: nowrap;
  height: var(--%NS%mat-toolbar-standard-height, 64px);
}
@media (max-width: 599px) {
  .mat-toolbar-row, .mat-toolbar-single-row {
    height: var(--%NS%mat-toolbar-mobile-height, 56px);
  }
}

.mat-toolbar-multiple-rows {
  display: flex;
  box-sizing: border-box;
  flex-direction: column;
  width: 100%;
  min-height: var(--%NS%mat-toolbar-standard-height, 64px);
}
@media (max-width: 599px) {
  .mat-toolbar-multiple-rows {
    min-height: var(--%NS%mat-toolbar-mobile-height, 56px);
  }
}
`],encapsulation:2})}return e})();var bi=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵmod=IE({type:e});static ɵinj=Jl({imports:[yt$1]})}return e})();function eo(e,o){e&1&&(Ei$1(0,`a`,5),PD(1,`Users`),Hc())}function no(e,o){if(e&1){let t=JE();Ei$1(0,`mat-toolbar`,0)(1,`a`,1)(2,`mat-icon`),PD(3,`dashboard`),Hc(),Ei$1(4,`span`),PD(5,`ProjectManagementApp`),Hc()(),Ei$1(6,`nav`,2)(7,`a`,3),PD(8,`Projects`),Hc(),Ei$1(9,`a`,4),PD(10,`Tasks`),Hc(),$E(11,eo,2,0,`a`,5),Hc(),Bp(12,`span`,6),Ei$1(13,`span`,7),PD(14),Hc(),Ei$1(15,`span`,8),PD(16),Hc(),Ei$1(17,`button`,9),zp(`click`,function(){bu(t);return _u(iD().logout())}),PD(18,`Log out`),Hc()()}if(e&2){let t=o;Bv(11),UE(t.role===`Admin`?11:-1),Bv(2),Vp(`data-role`,t.role),Bv(),ah(t.role),Bv(2),ah(t.fullName)}}var zt=class e{store=T$1(X$2);user=this.store.selectSignal(f.selectUser);logout(){this.store.dispatch(In.logoutRequested())}static ɵfac=function(t){return new(t||e)};static ɵcmp=yE({type:e,selectors:[[`app-shell-header`]],decls:1,vars:1,consts:[[`color`,`primary`,1,`shell-toolbar`],[`routerLink`,`/projects`,1,`brand`],[1,`shell-nav`],[`mat-button`,``,`routerLink`,`/projects`,`routerLinkActive`,`active-link`],[`mat-button`,``,`routerLink`,`/tasks`,`routerLinkActive`,`active-link`],[`mat-button`,``,`routerLink`,`/auth/admin-users`,`routerLinkActive`,`active-link`],[1,`spacer`],[1,`chip`],[1,`user-name`],[`mat-stroked-button`,``,3,`click`]],template:function(t,n){if(t&1&&$E(0,no,19,4,`mat-toolbar`,0),t&2){let i;UE((i=n.user())?0:-1,i)}},dependencies:[Qn$1,bc,bi,gi,er,tr,yt,wt],styles:[`.shell-toolbar[_ngcontent-%COMP%]{position:sticky;top:0;z-index:10;display:flex;flex-wrap:wrap;gap:8px 16px;row-gap:8px;height:auto;min-height:64px;padding:8px 16px}.brand[_ngcontent-%COMP%]{display:flex;align-items:center;gap:8px;color:inherit;text-decoration:none;font-weight:700;white-space:nowrap}.shell-nav[_ngcontent-%COMP%]{display:flex;gap:4px;flex-wrap:wrap}.active-link[_ngcontent-%COMP%]{background:#ffffff29;border-radius:4px}.spacer[_ngcontent-%COMP%]{flex:1 1 auto}.user-name[_ngcontent-%COMP%]{white-space:nowrap}.chip[data-role][_ngcontent-%COMP%]{background:#ffffffd9}`]})};Da(class e{static ɵfac=function(t){return new(t||e)};static ɵcmp=yE({type:e,selectors:[[`app-root`]],decls:4,vars:0,consts:[[1,`app-content`]],template:function(t,n){t&1&&(Bp(0,`app-shell-header`),Ei$1(1,`main`,0),Bp(2,`router-outlet`),Hc(),Bp(3,`app-notification`))},dependencies:[ii$1,Lt,zt],encapsulation:2})},mi).catch(e=>console.error(e));export{U$1 as A,n as B,re$1 as C,L as D,Ke$1 as E,yt as F,p as I,f as L,Ye$1 as M,u as N,N as O,wt as P,Dt$1 as R,pt$2 as S,I as T,dt$1 as _,je as a,ne$1 as b,$$1 as c,Ht as d,Kt as f,at$1 as g,_t$1 as h,_t as i,Xe$1 as j,Qe$1 as k,$t as l,Vt as m,V as n,qe as o,M as p,W as r,ti$1 as s,He as t,B as u,gt$1 as v,ut$2 as w,pe$1 as x,j$1 as y,bt$1 as z};