import{n as s,t as r}from"./chunk-C9yOwMO6.js";import{$t as _g,A as Hm,An as is,Ar as vg,B as KE,Br as xg,Bt as YD,Ct as TE,D as HD,Dn as i1,E as Gv,Ft as Wc,G as M$1,Gn as mt$1,Gr as zE,H as Ku,Hn as mg,Hr as ye$1,J as Mg,K as ME,Kn as n1,Kt as Z$3,L as JF,Ln as kg,Lr as wt$2,Mn as iy,Mr as wE,Mt as Up,Nn as j$1,Nr as wi$1,O as HI,On as ih,Or as vD,P as Ig,Pn as jl,Pt as Wa,Qn as pD,Qt as _,R as Jp,Rn as kn,Rt as Xi$1,Sn as gr,Sr as tu,St as T,Tn as hg,Tr as ue$1,U as Li$1,Ur as yg,V as KF,Vn as mD,Vr as yD,Vt as YE,W as Ll,Wn as mr,Wt as Yt$1,Xt as Zp,Y as Mo,Yn as o1,Yt as Zc,_ as Em,_n as fu,_t as Rg,an as bn,ar as qe$1,at as Ol,b as Fl,bn as gg,br as tg,cn as cw,cr as qp,ct as Pl,dn as e1,dr as rD,dt as Q$1,er as pg,f as Cm,fn as eh,fr as rs,ft as QE,gr as st$2,gt as Re$1,h as Dv,i as A$2,in as bh,ir as qc,it as Og,jn as iw,jt as Uo,kn as ir,kt as Uc,l as Bp,ln as de$1,lt as Pn,m as Dr,mn as fD,mr as s1,mt as Qi$1,n as $p,nn as ae$2,nt as Nu,o as Ae$2,ot as Op,pn as es,pt as Qc,qn as ns,qr as zp,s as Ag,sr as qh,st as Pi$1,t as $e,un as dh,ut as Pp,v as Er,vn as gD,vt as SD,w as Go,wr as uD,wt as Tg,xr as th,xt as Su,yt as Sg,zr as xe,zt as Xp}from"./chunk-CXUl5D6b.js";import{S as xr,_ as je$1,c as Wc$1,d as Ze,f as cu,g as ir$1,h as fu$1,i as Da,l as Xo,m as fi$1,o as Fe,p as du,t as $a,u as Zc$1,v as lu}from"./chunk-BO_m31o8.js";import{t as T$1}from"./chunk-CUUqB6nv.js";import{_ as w,a as P,c as an,d as nn,f as on,g as v,h as tn,i as Ge,l as b,m as rn,n as E,o as Xe$1,p as q$3,r as Fe$1,s as Z$4,t as B$1,u as en,v as xe$1}from"./chunk-CH_E2COh.js";import{B as z$1,C as bn$1,E as k$1,F as nr,I as pt$3,L as sn,M as mi$1,O as l$1,P as ni$2,S as ar,_ as Yn$1,a as Fe$2,b as Zo,c as Ot$1,d as St$1,g as Y$2,i as Ei$1,m as Ve$1,n as Dn,o as Go$1,s as M$2,t as Bt$1,u as Rt$1,v as Yo,w as ci$1,x as an$1,z as y$1}from"./chunk-DUInHNO1.js";var m=new A$2(`MAT_DATE_LOCALE`,{providedIn:`root`,factory:()=>T(Zc)});var a$1=`Method not implemented`;var l=class{locale;_localeChanges=new Q$1;localeChanges=this._localeChanges;setTime(t,e,r,i){throw new Error(a$1)}getHours(t){throw new Error(a$1)}getMinutes(t){throw new Error(a$1)}getSeconds(t){throw new Error(a$1)}parseTime(t,e){throw new Error(a$1)}addSeconds(t,e){throw new Error(a$1)}getValidDateOrNull(t){return this.isDateInstance(t)&&this.isValid(t)?t:null}deserialize(t){return t==null||this.isDateInstance(t)&&this.isValid(t)?t:this.invalid()}setLocale(t){this.locale=t,this._localeChanges.next()}compareDate(t,e){return this.getYear(t)-this.getYear(e)||this.getMonth(t)-this.getMonth(e)||this.getDate(t)-this.getDate(e)}compareTime(t,e){return this.getHours(t)-this.getHours(e)||this.getMinutes(t)-this.getMinutes(e)||this.getSeconds(t)-this.getSeconds(e)}sameDate(t,e){if(t&&e){let r=this.isValid(t),i=this.isValid(e);return r&&i?!this.compareDate(t,e):r==i}return t==e}sameTime(t,e){if(t&&e){let r=this.isValid(t),i=this.isValid(e);return r&&i?!this.compareTime(t,e):r==i}return t==e}clampDate(t,e,r){return e&&this.compareDate(t,e)<0?e:r&&this.compareDate(t,r)>0?r:t}};var d=new A$2(`mat-date-formats`);var n=class t{http=T(Xo);register(e){return this.http.post(`/api/auth/register`,e)}login(e){return this.http.post(`/api/auth/login`,e,{withCredentials:!0})}logout(){return this.http.post(`/api/auth/logout`,null,{withCredentials:!0})}refresh(){return this.http.post(`/api/auth/refresh`,null,{withCredentials:!0})}static ɵfac=function(i){return new(i||t)};static ɵprov=ae$2({token:t,factory:t.ɵfac,providedIn:`root`})};var K$2={dispatch:!0,functional:!1,useEffectsErrorHandler:!0};var h=`__@ngrx/effects_create__`;function Ct(t,r$8={}){let e=r$8.functional?t:t(),n=r(r({},K$2),r$8);return Object.defineProperty(e,h,{value:n}),e}function Y$1(t){return Object.getOwnPropertyNames(t).filter(n=>t[n]&&t[n].hasOwnProperty(h)?t[n][h].hasOwnProperty(`dispatch`):!1).map(n=>{let s=t[n][h];return r({propertyName:n},s)})}function J$3(t){return Y$1(t)}function U$3(t){return Object.getPrototypeOf(t)}function L$1(t){return!!t.constructor&&t.constructor.name!==`Object`&&t.constructor.name!==`Function`}function G$3(t){return typeof t==`function`}function X$2(t){return t.filter(G$3)}function q$2(t,r,e){let n=U$3(t),o=!!n&&n.constructor.name!==`Object`?n.constructor.name:null;return hg(...J$3(t).map(({propertyName:i,dispatch:V,useEffectsErrorHandler:B})=>{let S=typeof t[i]==`function`?t[i]():t[i],M=B?e(S,r):S;return V===!1?M.pipe(vg()):M.pipe(_g()).pipe(ue$1(z=>({effect:t[i],notification:z,propertyName:i,sourceName:o,sourceInstance:t})))}))}var Q=10;function H(t,r,e=Q){return t.pipe(ns(n=>(r&&r.handleError(n),e<=1?t:H(t,r,e-1))))}var bt$1=(()=>{class t extends _{constructor(e){super(),e&&(this.source=e)}lift(e){let n=new t;return n.source=this,n.operator=e,n}static{this.ɵfac=function(n){return new(n||t)(xe(B$1))}}static{this.ɵprov=ae$2({token:t,factory:t.ɵfac,providedIn:`root`})}}return t})();function Dt(...t){return Yt$1(r=>t.some(e=>typeof e==`string`?e===r.type:e.type===r.type))}var W$3=new A$2(`@ngrx/effects Effects Error Handler`,{providedIn:`root`,factory:()=>H});var tt$2=Ge(`@ngrx/effects/init`);function et$2(t,r){if(t.notification.kind===`N`){let e=t.notification.value;!nt$2(e)&&r.handleError(new Error(`Effect ${rt$2(t)} dispatched an invalid action: ${ot$3(e)}`))}}function nt$2(t){return typeof t!=`function`&&t&&t.type&&typeof t.type==`string`}function rt$2({propertyName:t,sourceInstance:r,sourceName:e}){let n=typeof r[t]==`function`;return!!e?`"${e}.${String(t)}${n?`()`:``}"`:`"${String(t)}()"`}function ot$3(t){try{return JSON.stringify(t)}catch{return t}}var st$1=`ngrxOnIdentifyEffects`;function it$2(t){return y(t,st$1)}var ft$3=`ngrxOnRunEffects`;function ct$2(t){return y(t,ft$3)}var ut$3=`ngrxOnInitEffects`;function at$2(t){return y(t,ut$3)}function y(t,r){return t&&r in t&&typeof t[r]==`function`}var k=(()=>{class t extends Q$1{constructor(e,n){super(),this.errorHandler=e,this.effectsErrorHandler=n}addEffects(e){this.next(e)}toActions(){return this.pipe(Tg(e=>L$1(e)?U$3(e):e),wt$2(e=>e.pipe(Tg(dt$2))),wt$2(e=>{return hg(e.pipe(Ol(o=>lt$2(this.errorHandler,this.effectsErrorHandler)(o)),ue$1(o=>(et$2(o,this.errorHandler),o.notification)),Yt$1(o=>o.kind===`N`&&o.value!=null),Ig()),e.pipe(rs(1),Yt$1(at$2),ue$1(o=>o.ngrxOnInitEffects())))}))}static{this.ɵfac=function(n){return new(n||t)(xe(st$2),xe(W$3))}}static{this.ɵprov=ae$2({token:t,factory:t.ɵfac,providedIn:`root`})}}return t})();function dt$2(t){return it$2(t)?t.ngrxOnIdentifyEffects():``}function lt$2(t,r){return e=>{let n=q$2(e,t,r);return ct$2(e)?e.ngrxOnRunEffects(n):n}}var Et=(()=>{class t{get isStarted(){return!!this.effectsSubscription}constructor(e,n){this.effectSources=e,this.store=n,this.effectsSubscription=null}start(){this.effectsSubscription||(this.effectsSubscription=this.effectSources.toActions().subscribe(this.store))}ngOnDestroy(){this.effectsSubscription&&(this.effectsSubscription.unsubscribe(),this.effectsSubscription=null)}static{this.ɵfac=function(n){return new(n||t)(xe(k),xe(Z$4))}}static{this.ɵprov=ae$2({token:t,factory:t.ɵfac,providedIn:`root`})}}return t})();function wt$1(...t){let r=t.flat();return Mo([X$2(r),fu(()=>{T(P),T(Fe$1,{optional:!0});let n=T(Et),s=T(k),o=!n.isStarted;o&&n.start();for(let c of r){let i=G$3(c)?T(c):c;s.addEffects(i)}o&&T(Z$4).dispatch(tt$2())})])}var o={accessToken:null,expiresAt:null,user:null,isLoading:!1,error:null};var f=en({name:`auth`,reducer:on(o,rn(an.loginSubmitted,r$5=>s(r({},r$5),{isLoading:!0,error:null})),rn(an.loginSuccess,an.tokenRefreshed,(r$6,{accessToken:l,expiresAt:u,user:c})=>s(r({},r$6),{isLoading:!1,accessToken:l,expiresAt:u,user:c,error:null})),rn(an.loginFailure,(r$7,{error:l})=>s(r({},r$7),{isLoading:!1,error:l})),rn(an.sessionCleared,an.logoutCompleted,()=>o))});var p=()=>{let c=T(Z$4),n=T(je$1);return c.selectSignal(f.selectAccessToken)()?!0:n.createUrlTree([`/auth/login`])};function V(i){return Error(`Unable to find icon with the name "${i}"`)}function X$1(){return Error(`Could not find HttpClient for use with Angular Material icons. Please add provideHttpClient() to your providers.`)}function q$1(i){return Error(`The URL provided to MatIconRegistry was not trusted as a resource URL via Angular's DomSanitizer. Attempted URL was "${i}".`)}function Y(i){return Error(`The literal provided to MatIconRegistry was not trusted as safe HTML by Angular's DomSanitizer. Attempted literal was "${i}".`)}var a=class{url;svgText;options;svgElement=null;constructor(l,t,e){this.url=l,this.svgText=t,this.options=e}};var K$1=(()=>{class i{_httpClient;_sanitizer;_errorHandler;_document;_svgIconConfigs=new Map;_iconSetConfigs=new Map;_cachedIconsByUrl=new Map;_inProgressUrlFetches=new Map;_fontCssClassesByAlias=new Map;_resolvers=[];_defaultFontSetClass=[`material-icons`,`mat-ligature-font`];constructor(t,e,n,o){this._httpClient=t,this._sanitizer=e,this._errorHandler=o,this._document=n}addSvgIcon(t,e,n){return this.addSvgIconInNamespace(``,t,e,n)}addSvgIconLiteral(t,e,n){return this.addSvgIconLiteralInNamespace(``,t,e,n)}addSvgIconInNamespace(t,e,n,o){return this._addSvgIconConfig(t,e,new a(n,null,o))}addSvgIconResolver(t){return this._resolvers.push(t),this}addSvgIconLiteralInNamespace(t,e,n,o){let r=this._sanitizer.sanitize(Z$3.HTML,n);if(!r)throw Y(n);let s=bn$1(r);return this._addSvgIconConfig(t,e,new a(``,s,o))}addSvgIconSet(t,e){return this.addSvgIconSetInNamespace(``,t,e)}addSvgIconSetLiteral(t,e){return this.addSvgIconSetLiteralInNamespace(``,t,e)}addSvgIconSetInNamespace(t,e,n){return this._addSvgIconSetConfig(t,new a(e,null,n))}addSvgIconSetLiteralInNamespace(t,e,n){let o=this._sanitizer.sanitize(Z$3.HTML,e);if(!o)throw Y(e);let r=bn$1(o);return this._addSvgIconSetConfig(t,new a(``,r,n))}registerFontClassAlias(t,e=t){return this._fontCssClassesByAlias.set(t,e),this}classNameForFontAlias(t){return this._fontCssClassesByAlias.get(t)||t}setDefaultFontSetClass(...t){return this._defaultFontSetClass=t,this}getDefaultFontSetClass(){return this._defaultFontSetClass}getSvgIconFromUrl(t){let e=this._sanitizer.sanitize(Z$3.RESOURCE_URL,t);if(!e)throw q$1(t);let n=this._cachedIconsByUrl.get(e);return n?Xi$1(C(n)):this._loadSvgIconFromConfig(new a(t,null)).pipe(jl(o=>this._cachedIconsByUrl.set(e,o)),ue$1(o=>C(o)))}getNamedSvgIcon(t,e=``){let n=J$2(e,t),o=this._svgIconConfigs.get(n);if(o)return this._getSvgFromConfig(o);if(o=this._getIconConfigFromResolvers(e,t),o)return this._svgIconConfigs.set(n,o),this._getSvgFromConfig(o);let r=this._iconSetConfigs.get(e);return r?this._getSvgFromIconSetConfigs(t,r):es(V(n))}ngOnDestroy(){this._resolvers=[],this._svgIconConfigs.clear(),this._iconSetConfigs.clear(),this._cachedIconsByUrl.clear()}_getSvgFromConfig(t){return t.svgText?Xi$1(C(this._svgElementFromConfig(t))):this._loadSvgIconFromConfig(t).pipe(ue$1(e=>C(e)))}_getSvgFromIconSetConfigs(t,e){let n=this._extractIconWithNameFromAnySet(t,e);if(n)return Xi$1(n);return pg(e.filter(r=>!r.svgText).map(r=>this._loadSvgIconSetFromConfig(r).pipe(ns(s=>{let f=`Loading icon set URL: ${this._sanitizer.sanitize(Z$3.RESOURCE_URL,r.url)} failed: ${s.message}`;return this._errorHandler.handleError(new Error(f)),Xi$1(null)})))).pipe(ue$1(()=>{let r=this._extractIconWithNameFromAnySet(t,e);if(!r)throw V(t);return r}))}_extractIconWithNameFromAnySet(t,e){for(let n=e.length-1;n>=0;n--){let o=e[n];if(o.svgText&&o.svgText.toString().indexOf(t)>-1){let r=this._svgElementFromConfig(o),s=this._extractSvgIconFromSet(r,t,o.options);if(s)return s}}return null}_loadSvgIconFromConfig(t){return this._fetchIcon(t).pipe(jl(e=>t.svgText=e),ue$1(()=>this._svgElementFromConfig(t)))}_loadSvgIconSetFromConfig(t){return t.svgText?Xi$1(null):this._fetchIcon(t).pipe(jl(e=>t.svgText=e))}_extractSvgIconFromSet(t,e,n){let o=t.querySelector(`[id="${e}"]`);if(!o)return null;let r=o.cloneNode(!0);if(r.removeAttribute(`id`),r.nodeName.toLowerCase()===`svg`)return this._setSvgAttributes(r,n);if(r.nodeName.toLowerCase()===`symbol`)return this._setSvgAttributes(this._toSvgElement(r),n);let s=this._svgElementFromString(bn$1(`<svg></svg>`));return s.appendChild(r),this._setSvgAttributes(s,n)}_svgElementFromString(t){let e=this._document.createElement(`DIV`);e.innerHTML=t;let n=e.querySelector(`svg`);if(!n)throw Error(`<svg> tag not found`);return n}_toSvgElement(t){let e=this._svgElementFromString(bn$1(`<svg></svg>`)),n=t.attributes;for(let o=0;o<n.length;o++){let{name:r,value:s}=n[o];r!==`id`&&e.setAttribute(r,s)}for(let o=0;o<t.childNodes.length;o++)t.childNodes[o].nodeType===this._document.ELEMENT_NODE&&e.appendChild(t.childNodes[o].cloneNode(!0));return e}_setSvgAttributes(t,e){return t.setAttribute(`fit`,``),t.setAttribute(`height`,`100%`),t.setAttribute(`width`,`100%`),t.setAttribute(`preserveAspectRatio`,`xMidYMid meet`),t.setAttribute(`focusable`,`false`),e&&e.viewBox&&t.setAttribute(`viewBox`,e.viewBox),t}_fetchIcon(t){let{url:e,options:n}=t,o=n?.withCredentials??!1;if(!this._httpClient)throw X$1();if(e==null)throw Error(`Cannot fetch icon from URL "${e}".`);let r=this._sanitizer.sanitize(Z$3.RESOURCE_URL,e);if(!r)throw q$1(e);let s=this._inProgressUrlFetches.get(r);if(s)return s;let h=this._httpClient.get(r,{responseType:`text`,withCredentials:o}).pipe(ue$1(f=>bn$1(f)),Ll(()=>this._inProgressUrlFetches.delete(r)),is());return this._inProgressUrlFetches.set(r,h),h}_addSvgIconConfig(t,e,n){return this._svgIconConfigs.set(J$2(t,e),n),this}_addSvgIconSetConfig(t,e){let n=this._iconSetConfigs.get(t);return n?n.push(e):this._iconSetConfigs.set(t,[e]),this}_svgElementFromConfig(t){if(!t.svgElement){let e=this._svgElementFromString(t.svgText);this._setSvgAttributes(e,t.options),t.svgElement=e}return t.svgElement}_getIconConfigFromResolvers(t,e){for(let n=0;n<this._resolvers.length;n++){let o=this._resolvers[n](e,t);if(o)return Z$2(o)?new a(o.url,null,o.options):new a(o,null)}}static ɵfac=function(e){return new(e||i)(xe(Xo,8),xe(fu$1),xe(ir,8),xe(st$2))};static ɵprov=ae$2({token:i,factory:i.ɵfac,providedIn:`root`})}return i})();function C(i){return i.cloneNode(!0)}function J$2(i,l){return i+`:`+l}function Z$2(i){return!!(i.url&&i.options)}var tt$1=[`*`];var et$1=new A$2(`MAT_ICON_DEFAULT_OPTIONS`);var nt$1=new A$2(`mat-icon-location`,{providedIn:`root`,factory:()=>{let i=T(ir),l=i?i.location:null;return{getPathname:()=>l?l.pathname+l.search:``}}});var G$2=[`clip-path`,`color-profile`,`src`,`cursor`,`fill`,`filter`,`marker`,`marker-start`,`marker-mid`,`marker-end`,`mask`,`stroke`];var ot$2=G$2.map(i=>`[${i}]`).join(`, `);var rt$1=/^url\(['"]?#(.*?)['"]?\)$/;var wt=(()=>{class i{_elementRef=T(Dr);_iconRegistry=T(K$1);_location=T(nt$1);_errorHandler=T(st$2);_defaultColor;get color(){return this._color||this._defaultColor}set color(t){this._color=t}_color;inline=!1;get svgIcon(){return this._svgIcon}set svgIcon(t){t!==this._svgIcon&&(t?this._updateSvgIcon(t):this._svgIcon&&this._clearSvgElement(),this._svgIcon=t)}_svgIcon;get fontSet(){return this._fontSet}set fontSet(t){let e=this._cleanupFontValue(t);e!==this._fontSet&&(this._fontSet=e,this._updateFontIconClasses())}_fontSet;get fontIcon(){return this._fontIcon}set fontIcon(t){let e=this._cleanupFontValue(t);e!==this._fontIcon&&(this._fontIcon=e,this._updateFontIconClasses())}_fontIcon;_previousFontSetClass=[];_previousFontIconClass;_svgName=null;_svgNamespace=null;_previousPath;_elementsWithExternalReferences;_currentIconFetch=j$1.EMPTY;constructor(){let t=T(new bh(`aria-hidden`),{optional:!0}),e=T(et$1,{optional:!0});e&&(e.color&&(this.color=this._defaultColor=e.color),e.fontSet&&(this.fontSet=e.fontSet)),t||this._elementRef.nativeElement.setAttribute(`aria-hidden`,`true`)}_splitIconName(t){if(!t)return[``,``];let e=t.split(`:`);switch(e.length){case 1:return[``,e[0]];case 2:return e;default:throw Error(`Invalid icon name: "${t}"`)}}ngOnInit(){this._updateFontIconClasses()}ngAfterViewChecked(){let t=this._elementsWithExternalReferences;if(t&&t.size){let e=this._location.getPathname();e!==this._previousPath&&(this._previousPath=e,this._prependPathToReferences(e))}}ngOnDestroy(){this._currentIconFetch.unsubscribe(),this._elementsWithExternalReferences&&this._elementsWithExternalReferences.clear()}_usingFontIcon(){return!this.svgIcon}_setSvgElement(t){this._clearSvgElement();let e=this._location.getPathname();this._previousPath=e,this._cacheChildrenWithExternalReferences(t),this._prependPathToReferences(e),this._elementRef.nativeElement.appendChild(t)}_clearSvgElement(){let t=this._elementRef.nativeElement,e=t.childNodes.length;for(this._elementsWithExternalReferences&&this._elementsWithExternalReferences.clear();e--;){let n=t.childNodes[e];(n.nodeType!==1||n.nodeName.toLowerCase()===`svg`)&&n.remove()}}_updateFontIconClasses(){if(!this._usingFontIcon())return;let t=this._elementRef.nativeElement,e=(this.fontSet?this._iconRegistry.classNameForFontAlias(this.fontSet).split(/ +/):this._iconRegistry.getDefaultFontSetClass()).filter(n=>n.length>0);this._previousFontSetClass.forEach(n=>t.classList.remove(n)),e.forEach(n=>t.classList.add(n)),this._previousFontSetClass=e,this.fontIcon!==this._previousFontIconClass&&!e.includes(`mat-ligature-font`)&&(this._previousFontIconClass&&t.classList.remove(this._previousFontIconClass),this.fontIcon&&t.classList.add(this.fontIcon),this._previousFontIconClass=this.fontIcon)}_cleanupFontValue(t){return typeof t==`string`?t.trim().split(` `)[0]:t}_prependPathToReferences(t){let e=this._elementsWithExternalReferences;e&&e.forEach((n,o)=>{n.forEach(r=>{o.setAttribute(r.name,`url('${t}#${r.value}')`)})})}_cacheChildrenWithExternalReferences(t){let e=t.querySelectorAll(ot$2),n=this._elementsWithExternalReferences=this._elementsWithExternalReferences||new Map;for(let o=0;o<e.length;o++)G$2.forEach(r=>{let s=e[o],h=s.getAttribute(r),f=h?h.match(rt$1):null;if(f){let p=n.get(s);p||(p=[],n.set(s,p)),p.push({name:r,value:f[1]})}})}_updateSvgIcon(t){if(this._svgNamespace=null,this._svgName=null,this._currentIconFetch.unsubscribe(),t){let[e,n]=this._splitIconName(t);e&&(this._svgNamespace=e),n&&(this._svgName=n),this._currentIconFetch=this._iconRegistry.getNamedSvgIcon(n,e).pipe(rs(1)).subscribe(o=>this._setSvgElement(o),o=>{let r=`Error retrieving icon ${e}:${n}! ${o.message}`;this._errorHandler.handleError(new Error(r))})}}static ɵfac=function(e){return new(e||i)};static ɵcmp=wE({type:i,selectors:[[`mat-icon`]],hostAttrs:[`role`,`img`,1,`mat-icon`,`notranslate`],hostVars:10,hostBindings:function(e,n){e&2&&(Bp(`data-mat-icon-type`,n._usingFontIcon()?`font`:`svg`)(`data-mat-icon-name`,n._svgName||n.fontIcon)(`data-mat-icon-namespace`,n._svgNamespace||n.fontSet)(`fontIcon`,n._usingFontIcon()?n.fontIcon:null),SD(n.color?`mat-`+n.color:``),ih(`mat-icon-inline`,n.inline)(`mat-icon-no-color`,n.color!==`primary`&&n.color!==`accent`&&n.color!==`warn`))},inputs:{color:`color`,inline:[2,`inline`,`inline`,n1],svgIcon:`svgIcon`,fontSet:`fontSet`,fontIcon:`fontIcon`},exportAs:[`matIcon`],ngContentSelectors:tt$1,decls:1,vars:0,template:function(e,n){e&1&&(fD(),pD(0))},styles:[`mat-icon, mat-icon.mat-primary, mat-icon.mat-accent, mat-icon.mat-warn {
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
`],encapsulation:2})}return i})();var yt$1=(()=>{class i{static ɵfac=function(e){return new(e||i)};static ɵmod=TE({type:i});static ɵinj=tu({imports:[St$1]})}return i})();var J$1=(()=>{class t{_animationsDisabled=z$1();state=`unchecked`;disabled=!1;appearance=`full`;static ɵfac=function(o){return new(o||t)};static ɵcmp=wE({type:t,selectors:[[`mat-pseudo-checkbox`]],hostAttrs:[1,`mat-pseudo-checkbox`],hostVars:12,hostBindings:function(o,n){o&2&&ih(`mat-pseudo-checkbox-indeterminate`,n.state===`indeterminate`)(`mat-pseudo-checkbox-checked`,n.state===`checked`)(`mat-pseudo-checkbox-disabled`,n.disabled)(`mat-pseudo-checkbox-minimal`,n.appearance===`minimal`)(`mat-pseudo-checkbox-full`,n.appearance===`full`)(`_mat-animation-noopable`,n._animationsDisabled)},inputs:{state:`state`,disabled:`disabled`,appearance:`appearance`},decls:0,vars:0,template:function(o,n){},styles:[`.mat-pseudo-checkbox {
  border-radius: 2px;
  cursor: pointer;
  display: inline-block;
  vertical-align: middle;
  box-sizing: border-box;
  position: relative;
  flex-shrink: 0;
  transition: border-color 90ms cubic-bezier(0, 0, 0.2, 0.1), background-color 90ms cubic-bezier(0, 0, 0.2, 0.1);
}
.mat-pseudo-checkbox::after {
  position: absolute;
  opacity: 0;
  content: "";
  border-bottom: 2px solid currentColor;
  transition: opacity 90ms cubic-bezier(0, 0, 0.2, 0.1);
}
.mat-pseudo-checkbox._mat-animation-noopable {
  transition: none !important;
  animation: none !important;
}
.mat-pseudo-checkbox._mat-animation-noopable::after {
  transition: none;
}

.mat-pseudo-checkbox-disabled {
  cursor: default;
}

.mat-pseudo-checkbox-indeterminate::after {
  left: 1px;
  opacity: 1;
  border-radius: 2px;
}

.mat-pseudo-checkbox-checked::after {
  left: 1px;
  border-left: 2px solid currentColor;
  transform: rotate(-45deg);
  opacity: 1;
  box-sizing: content-box;
}

.mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-checked::after, .mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-indeterminate::after {
  color: var(--%NS%mat-pseudo-checkbox-minimal-selected-checkmark-color, var(--%NS%mat-sys-primary));
}
.mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-checked.mat-pseudo-checkbox-disabled::after, .mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-indeterminate.mat-pseudo-checkbox-disabled::after {
  color: var(--%NS%mat-pseudo-checkbox-minimal-disabled-selected-checkmark-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}

.mat-pseudo-checkbox-full {
  border-color: var(--%NS%mat-pseudo-checkbox-full-unselected-icon-color, var(--%NS%mat-sys-on-surface-variant));
  border-width: 2px;
  border-style: solid;
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-disabled {
  border-color: var(--%NS%mat-pseudo-checkbox-full-disabled-unselected-icon-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked, .mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate {
  background-color: var(--%NS%mat-pseudo-checkbox-full-selected-icon-color, var(--%NS%mat-sys-primary));
  border-color: transparent;
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked::after, .mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate::after {
  color: var(--%NS%mat-pseudo-checkbox-full-selected-checkmark-color, var(--%NS%mat-sys-on-primary));
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked.mat-pseudo-checkbox-disabled, .mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate.mat-pseudo-checkbox-disabled {
  background-color: var(--%NS%mat-pseudo-checkbox-full-disabled-selected-icon-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked.mat-pseudo-checkbox-disabled::after, .mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate.mat-pseudo-checkbox-disabled::after {
  color: var(--%NS%mat-pseudo-checkbox-full-disabled-selected-checkmark-color, var(--%NS%mat-sys-surface));
}

.mat-pseudo-checkbox {
  width: 18px;
  height: 18px;
}

.mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-checked::after {
  width: 14px;
  height: 6px;
  transform-origin: center;
  top: -4.2426406871px;
  left: 0;
  bottom: 0;
  right: 0;
  margin: auto;
}
.mat-pseudo-checkbox-minimal.mat-pseudo-checkbox-indeterminate::after {
  top: 8px;
  width: 16px;
}

.mat-pseudo-checkbox-full.mat-pseudo-checkbox-checked::after {
  width: 10px;
  height: 4px;
  transform-origin: center;
  top: -2.8284271247px;
  left: 0;
  bottom: 0;
  right: 0;
  margin: auto;
}
.mat-pseudo-checkbox-full.mat-pseudo-checkbox-indeterminate::after {
  top: 6px;
  width: 12px;
}
`],encapsulation:2})}return t})();var ee$1=[`text`];var te=[[[`mat-icon`]],`*`];var ne$2=[`mat-icon`,`*`];function oe$2(t,i){if(t&1&&Up(0,`mat-pseudo-checkbox`,1),t&2){let e=uD();$p(`disabled`,e.disabled)(`state`,e.selected?`checked`:`unchecked`)}}function ie$2(t,i){if(t&1&&Up(0,`mat-pseudo-checkbox`,3),t&2)$p(`disabled`,uD().disabled)}function ae$1(t,i){if(t&1&&(wi$1(0,`span`,4),HD(1),Uc()),t&2){let e=uD();Gv(),Qc(`(`,e.group.label,`)`)}}var re$2=new A$2(`MAT_OPTION_PARENT_COMPONENT`);var se$2=new A$2(`MatOptgroup`);var N$1=class{source;isUserInput;constructor(i,e=!1){this.source=i,this.isUserInput=e}};var W$2=(()=>{class t{_element=T(Dr);_changeDetectorRef=T(e1);_parent=T(re$2,{optional:!0});group=T(se$2,{optional:!0});_signalDisableRipple=!1;_selected=!1;_active=!1;_mostRecentViewValue=``;get multiple(){return this._parent&&this._parent.multiple}get selected(){return this._selected}value;id=T(Bt$1).getId(`mat-option-`);get disabled(){return this.group&&this.group.disabled||this._disabled()}set disabled(e){this._disabled.set(e)}_disabled=Uo(!1);get disableRipple(){return this._signalDisableRipple?this._parent.disableRipple():!!this._parent?.disableRipple}get hideSingleSelectionIndicator(){return!!(this._parent&&this._parent.hideSingleSelectionIndicator)}onSelectionChange=new qe$1;_text;_stateChanges=new Q$1;constructor(){let e=T(M$2);e.load(an$1),e.load(pt$3),this._signalDisableRipple=!!this._parent&&Go(this._parent.disableRipple)}get active(){return this._active}get viewValue(){return(this._text?.nativeElement.textContent||``).trim()}select(e=!0){this._selected||(this._selected=!0,this._changeDetectorRef.markForCheck(),e&&this._emitSelectionChangeEvent())}deselect(e=!0){this._selected&&(this._selected=!1,this._changeDetectorRef.markForCheck(),e&&this._emitSelectionChangeEvent())}focus(e,o){let n=this._getHostElement();typeof n.focus==`function`&&n.focus(o)}setActiveStyles(){this._active||(this._active=!0,this._changeDetectorRef.markForCheck())}setInactiveStyles(){this._active&&(this._active=!1,this._changeDetectorRef.markForCheck())}getLabel(){return this.viewValue}_handleKeydown(e){(e.keyCode===13||e.keyCode===32)&&!Ve$1(e)&&(this._selectViaInteraction(),e.preventDefault())}_selectViaInteraction(){this.disabled||(this._selected=this.multiple?!this._selected:!0,this._changeDetectorRef.markForCheck(),this._emitSelectionChangeEvent(!0))}_getTabIndex(){return this.disabled?`-1`:`0`}_getHostElement(){return this._element.nativeElement}ngAfterViewChecked(){if(this._selected){let e=this.viewValue;e!==this._mostRecentViewValue&&(this._mostRecentViewValue&&this._stateChanges.next(),this._mostRecentViewValue=e)}}ngOnDestroy(){this._stateChanges.complete()}_emitSelectionChangeEvent(e=!1){this.onSelectionChange.emit(new N$1(this,e))}static ɵfac=function(o){return new(o||t)};static ɵcmp=wE({type:t,selectors:[[`mat-option`]],viewQuery:function(o,n){if(o&1&&Xp(ee$1,7),o&2){let a;gD(a=mD())&&(n._text=a.first)}},hostAttrs:[`role`,`option`,1,`mat-mdc-option`,`mdc-list-item`],hostVars:11,hostBindings:function(o,n){o&1&&Zp(`click`,function(){return n._selectViaInteraction()})(`keydown`,function(s){return n._handleKeydown(s)}),o&2&&(zp(`id`,n.id),Bp(`aria-selected`,n.selected)(`aria-disabled`,n.disabled.toString()),ih(`mdc-list-item--selected`,n.selected)(`mat-mdc-option-multiple`,n.multiple)(`mat-mdc-option-active`,n.active)(`mdc-list-item--disabled`,n.disabled))},inputs:{value:`value`,id:`id`,disabled:[2,`disabled`,`disabled`,n1]},outputs:{onSelectionChange:`onSelectionChange`},exportAs:[`matOption`],ngContentSelectors:ne$2,decls:8,vars:5,consts:[[`text`,``],[`aria-hidden`,`true`,1,`mat-mdc-option-pseudo-checkbox`,3,`disabled`,`state`],[1,`mdc-list-item__primary-text`],[`state`,`checked`,`aria-hidden`,`true`,`appearance`,`minimal`,1,`mat-mdc-option-pseudo-checkbox`,3,`disabled`],[1,`cdk-visually-hidden`],[`aria-hidden`,`true`,`mat-ripple`,``,1,`mat-mdc-option-ripple`,`mat-focus-indicator`,3,`matRippleTrigger`,`matRippleDisabled`]],template:function(o,n){o&1&&(fD(te),zE(0,oe$2,1,2,`mat-pseudo-checkbox`,1),pD(1),wi$1(2,`span`,2,0),pD(4,1),Uc(),zE(5,ie$2,1,1,`mat-pseudo-checkbox`,3),zE(6,ae$1,2,1,`span`,4),Up(7,`div`,5)),o&2&&(QE(n.multiple?0:-1),Gv(5),QE(!n.multiple&&n.selected&&!n.hideSingleSelectionIndicator?5:-1),Gv(),QE(n.group&&n.group._inert?6:-1),Gv(),$p(`matRippleTrigger`,n._getHostElement())(`matRippleDisabled`,n.disabled||n.disableRipple))},dependencies:[J$1,Ei$1],styles:[`.mat-mdc-option {
  -webkit-user-select: none;
  user-select: none;
  -moz-osx-font-smoothing: grayscale;
  -webkit-font-smoothing: antialiased;
  display: flex;
  position: relative;
  align-items: center;
  justify-content: flex-start;
  overflow: hidden;
  min-height: 48px;
  padding: 0 16px;
  cursor: pointer;
  -webkit-tap-highlight-color: transparent;
  color: var(--%NS%mat-option-label-text-color, var(--%NS%mat-sys-on-surface));
  font-family: var(--%NS%mat-option-label-text-font, var(--%NS%mat-sys-label-large-font));
  line-height: var(--%NS%mat-option-label-text-line-height, var(--%NS%mat-sys-label-large-line-height));
  font-size: var(--%NS%mat-option-label-text-size, var(--%NS%mat-sys-body-large-size));
  letter-spacing: var(--%NS%mat-option-label-text-tracking, var(--%NS%mat-sys-label-large-tracking));
  font-weight: var(--%NS%mat-option-label-text-weight, var(--%NS%mat-sys-body-large-weight));
}
.mat-mdc-option:hover:not(.mdc-list-item--disabled) {
  background-color: var(--%NS%mat-option-hover-state-layer-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) calc(var(--%NS%mat-sys-hover-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-option:focus.mdc-list-item, .mat-mdc-option.mat-mdc-option-active.mdc-list-item {
  background-color: var(--%NS%mat-option-focus-state-layer-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) calc(var(--%NS%mat-sys-focus-state-layer-opacity) * 100%), transparent));
  outline: 0;
}
.mat-mdc-option.mdc-list-item--%NS%selected:not(.mdc-list-item--disabled):not(.mat-mdc-option-active, .mat-mdc-option-multiple, :focus, :hover) {
  background-color: var(--%NS%mat-option-selected-state-layer-color, var(--%NS%mat-sys-secondary-container));
}
.mat-mdc-option.mdc-list-item--%NS%selected:not(.mdc-list-item--disabled):not(.mat-mdc-option-active, .mat-mdc-option-multiple, :focus, :hover) .mdc-list-item__primary-text {
  color: var(--%NS%mat-option-selected-state-label-text-color, var(--%NS%mat-sys-on-secondary-container));
}
.mat-mdc-option .mat-pseudo-checkbox {
  --%NS%mat-pseudo-checkbox-minimal-selected-checkmark-color: var(--%NS%mat-option-selected-state-label-text-color, var(--%NS%mat-sys-on-secondary-container));
}
.mat-mdc-option.mdc-list-item {
  align-items: center;
  background: transparent;
}
.mat-mdc-option.mdc-list-item--disabled {
  cursor: default;
  pointer-events: none;
}
.mat-mdc-option.mdc-list-item--disabled .mat-mdc-option-pseudo-checkbox, .mat-mdc-option.mdc-list-item--disabled .mdc-list-item__primary-text, .mat-mdc-option.mdc-list-item--disabled > mat-icon {
  opacity: 0.38;
}
.mat-mdc-optgroup .mat-mdc-option:not(.mat-mdc-option-multiple) {
  padding-left: 32px;
}
[dir=rtl] .mat-mdc-optgroup .mat-mdc-option:not(.mat-mdc-option-multiple) {
  padding-left: 16px;
  padding-right: 32px;
}
.mat-mdc-option .mat-icon,
.mat-mdc-option .mat-pseudo-checkbox-full {
  margin-right: 16px;
  flex-shrink: 0;
}
[dir=rtl] .mat-mdc-option .mat-icon,
[dir=rtl] .mat-mdc-option .mat-pseudo-checkbox-full {
  margin-right: 0;
  margin-left: 16px;
}
.mat-mdc-option .mat-pseudo-checkbox-minimal {
  margin-left: 16px;
  flex-shrink: 0;
}
[dir=rtl] .mat-mdc-option .mat-pseudo-checkbox-minimal {
  margin-right: 16px;
  margin-left: 0;
}
.mat-mdc-option .mat-mdc-option-ripple {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  pointer-events: none;
}
.mat-mdc-option .mdc-list-item__primary-text {
  white-space: normal;
  font-size: inherit;
  font-weight: inherit;
  letter-spacing: inherit;
  line-height: inherit;
  font-family: inherit;
  text-decoration: inherit;
  text-transform: inherit;
  margin-right: auto;
}
[dir=rtl] .mat-mdc-option .mdc-list-item__primary-text {
  margin-right: 0;
  margin-left: auto;
}
@media (forced-colors: active) {
  .mat-mdc-option.mdc-list-item--%NS%selected:not(:has(.mat-mdc-option-pseudo-checkbox))::after {
    content: "";
    position: absolute;
    top: 50%;
    right: 16px;
    transform: translateY(-50%);
    width: 10px;
    height: 0;
    border-bottom: solid 10px;
    border-radius: 10px;
  }
  [dir=rtl] .mat-mdc-option.mdc-list-item--%NS%selected:not(:has(.mat-mdc-option-pseudo-checkbox))::after {
    right: auto;
    left: 16px;
  }
}

.mat-mdc-option-multiple {
  --%NS%mat-list-list-item-selected-container-color: var(--%NS%mat-list-list-item-container-color, transparent);
}

.mat-mdc-option-active .mat-focus-indicator::before {
  content: "";
}
`],encapsulation:2})}return t})();function Me(t,i,e){if(e.length){let o=i.toArray(),n=e.toArray(),a=0;for(let s=0;s<t+1;s++)o[s].group&&o[s].group===n[a]&&a++;return a}return 0}function we(t,i,e,o){return t<e?t:t+i>e+o?Math.max(0,t-o+i):e}var X=(()=>{class t{static ɵfac=function(o){return new(o||t)};static ɵmod=TE({type:t});static ɵinj=tu({imports:[St$1]})}return t})();var Ae$1=(()=>{class t{static ɵfac=function(o){return new(o||t)};static ɵmod=TE({type:t});static ɵinj=tu({imports:[sn,X,W$2,St$1]})}return t})();function U$2(i){return i&&typeof i.connect==`function`&&!(i instanceof Qi$1)}var u=(function(i){return i[i.REPLACED=0]=`REPLACED`,i[i.INSERTED=1]=`INSERTED`,i[i.MOVED=2]=`MOVED`,i[i.REMOVED=3]=`REMOVED`,i})(u||{});var L=class{viewCacheSize=20;_viewCache=[];applyChanges(s,e,t,r,n){s.forEachOperation((o,d,f)=>{let _,p;if(o.previousIndex==null){let P=()=>t(o,d,f);_=this._insertView(P,f,e,r(o)),p=_?u.INSERTED:u.REPLACED}else f==null?(this._detachAndCacheView(d,e),p=u.REMOVED):(_=this._moveView(d,f,e,r(o)),p=u.MOVED);n&&n({context:_?.context,operation:p,record:o})})}detach(){for(let s of this._viewCache)s.destroy();this._viewCache=[]}_insertView(s,e,t,r){let n=this._insertViewFromCache(e,t);if(n){n.context.$implicit=r;return}let o=s();return t.createEmbeddedView(o.templateRef,o.context,o.index)}_detachAndCacheView(s,e){let t=e.detach(s);this._maybeCacheView(t,e)}_moveView(s,e,t,r){let n=t.get(s);return t.move(n,e),n.context.$implicit=r,n}_maybeCacheView(s,e){if(this._viewCache.length<this.viewCacheSize)this._viewCache.push(s);else{let t=e.indexOf(s);t===-1?s.destroy():e.remove(t)}}_insertViewFromCache(s,e){let t=this._viewCache.pop();return t&&e.insert(t,s),t||null}};var A$1=20;var N=(()=>{class i{_ngZone=T(Ae$2);_platform=T(l$1);_renderer=T(mr).createRenderer(null,null);_cleanupGlobalListener;_scrolled=new Q$1;_scrolledCount=0;scrollContainers=new Map;register(e){this.scrollContainers.has(e)||this.scrollContainers.set(e,e.elementScrolled().subscribe(()=>this._scrolled.next(e)))}deregister(e){let t=this.scrollContainers.get(e);t&&(t.unsubscribe(),this.scrollContainers.delete(e))}scrolled(e=A$1){return this._platform.isBrowser?new _(t=>{this._cleanupGlobalListener||(this._cleanupGlobalListener=this._ngZone.runOutsideAngular(()=>this._renderer.listen(`document`,`scroll`,()=>this._scrolled.next())));let r=e>0?this._scrolled.pipe(gg(e)).subscribe(t):this._scrolled.subscribe(t);return this._scrolledCount++,()=>{r.unsubscribe(),this._scrolledCount--,this._scrolledCount||(this._cleanupGlobalListener?.(),this._cleanupGlobalListener=void 0)}}):Xi$1()}ngOnDestroy(){this._cleanupGlobalListener?.(),this._cleanupGlobalListener=void 0,this.scrollContainers.forEach((e,t)=>this.deregister(t)),this._scrolled.complete()}ancestorScrolled(e,t){let r=this.getAncestorScrollContainers(e);return this.scrolled(t).pipe(Yt$1(n=>!n||r.indexOf(n)>-1))}getAncestorScrollContainers(e){let t=[];return this.scrollContainers.forEach((r,n)=>{this._targetContainsElement(n,e)&&t.push(n)}),t}_targetContainsElement(e,t){let r=k$1(t),n=e.getElementRef().nativeElement;do if(r==n)return!0;while(r=r.parentElement);return!1}static ɵfac=function(t){return new(t||i)};static ɵprov=Er({token:i,factory:i.ɵfac})}return i})();var Ye=(()=>{class i{elementRef=T(Dr);scrollDispatcher=T(N);ngZone=T(Ae$2);dir=T(Yn$1,{optional:!0});_scrollElement=this.elementRef.nativeElement;_destroyed=new Q$1;_renderer=T(Wa);_cleanupScroll;_elementScrolled=new Q$1;ngOnInit(){this._cleanupScroll=this.ngZone.runOutsideAngular(()=>this._renderer.listen(this._scrollElement,`scroll`,e=>this._elementScrolled.next(e))),this.scrollDispatcher.register(this)}ngOnDestroy(){this._cleanupScroll?.(),this._elementScrolled.complete(),this.scrollDispatcher.deregister(this),this._destroyed.next(),this._destroyed.complete()}elementScrolled(){return this._elementScrolled}getElementRef(){return this.elementRef}scrollTo(e){let t=this.elementRef.nativeElement,r=this.dir&&this.dir.value==`rtl`;e.left??=r?e.end:e.start,e.right??=r?e.start:e.end,e.bottom!=null&&(e.top=t.scrollHeight-t.clientHeight-e.bottom),r&&Go$1()!=Y$2.NORMAL?(e.left!=null&&(e.right=t.scrollWidth-t.clientWidth-e.left),Go$1()==Y$2.INVERTED?e.left=e.right:Go$1()==Y$2.NEGATED&&(e.left=e.right?-e.right:e.right)):e.right!=null&&(e.left=t.scrollWidth-t.clientWidth-e.right),this._applyScrollToOptions(e)}_applyScrollToOptions(e){let t=this.elementRef.nativeElement;Zo()?t.scrollTo(e):(e.top!=null&&(t.scrollTop=e.top),e.left!=null&&(t.scrollLeft=e.left))}measureScrollOffset(e){let t=`left`,r=`right`,n=this.elementRef.nativeElement;if(e==`top`)return n.scrollTop;if(e==`bottom`)return n.scrollHeight-n.clientHeight-n.scrollTop;let o=this.dir&&this.dir.value==`rtl`;return e==`start`?e=o?r:t:e==`end`&&(e=o?t:r),o&&Go$1()==Y$2.INVERTED?e==t?n.scrollWidth-n.clientWidth-n.scrollLeft:n.scrollLeft:o&&Go$1()==Y$2.NEGATED?e==t?n.scrollLeft+n.scrollWidth-n.clientWidth:-n.scrollLeft:e==t?n.scrollLeft:n.scrollWidth-n.clientWidth-n.scrollLeft}static ɵfac=function(t){return new(t||i)};static ɵdir=ME({type:i,selectors:[[``,`cdk-scrollable`,``],[``,`cdkScrollable`,``]]})}return i})();var W$1=20;var Qe$1=(()=>{class i{_platform=T(l$1);_listeners;_viewportSize=null;_change=new Q$1;_document=T(ir);constructor(){let e=T(Ae$2),t=T(mr).createRenderer(null,null);e.runOutsideAngular(()=>{if(this._platform.isBrowser){let r=n=>this._change.next(n);this._listeners=[t.listen(`window`,`resize`,r),t.listen(`window`,`orientationchange`,r)]}this.change().subscribe(()=>this._viewportSize=null)})}ngOnDestroy(){this._listeners?.forEach(e=>e()),this._change.complete()}getViewportSize(){this._viewportSize||this._updateViewportSize();let e={width:this._viewportSize.width,height:this._viewportSize.height};return this._platform.isBrowser||(this._viewportSize=null),e}getViewportRect(){let e=this.getViewportScrollPosition(),{width:t,height:r}=this.getViewportSize();return{top:e.top,left:e.left,bottom:e.top+r,right:e.left+t,height:r,width:t}}getViewportScrollPosition(){if(!this._platform.isBrowser)return{top:0,left:0};let e=this._document,t=this._getWindow(),r=e.documentElement,n=r.getBoundingClientRect();return{top:-n.top||e.body?.scrollTop||t.scrollY||r.scrollTop||0,left:-n.left||e.body?.scrollLeft||t.scrollX||r.scrollLeft||0}}change(e=W$1){return e>0?this._change.pipe(gg(e)):this._change}_getWindow(){return this._document.defaultView||window}_updateViewportSize(){let e=this._getWindow();this._viewportSize=this._platform.isBrowser?{width:e.innerWidth,height:e.innerHeight}:{width:0,height:0}}static ɵfac=function(t){return new(t||i)};static ɵprov=Er({token:i,factory:i.ɵfac})}return i})();var Ke$1=new A$2(`CDK_VIRTUAL_SCROLL_VIEWPORT`);var I=(()=>{class i{static ɵfac=function(t){return new(t||i)};static ɵmod=TE({type:i});static ɵinj=tu({})}return i})();var Xe=(()=>{class i{static ɵfac=function(t){return new(t||i)};static ɵmod=TE({type:i});static ɵinj=tu({imports:[St$1,I,St$1,I]})}return i})();var D=class{_attachedHost=null;attach(t){return this._attachedHost=t,t.attach(this)}detach(){let t=this._attachedHost;t!=null&&(this._attachedHost=null,t.detach())}get isAttached(){return this._attachedHost!=null}setAttachedHost(t){this._attachedHost=t}};var at$1=class extends D{component;viewContainerRef;injector;projectableNodes;bindings;directives;constructor(t,e,i,s,n,r){super(),this.component=t,this.viewContainerRef=e,this.injector=i,this.projectableNodes=s,this.bindings=n||null,this.directives=r||null}};var M=class extends D{templateRef;viewContainerRef;context;injector;constructor(t,e,i,s){super(),this.templateRef=t,this.viewContainerRef=e,this.context=i,this.injector=s}get origin(){return this.templateRef.elementRef}attach(t,e=this.context){return this.context=e,super.attach(t)}detach(){return this.context=void 0,super.detach()}};var lt$1=class extends D{element;constructor(t){super(),this.element=t instanceof Dr?t.nativeElement:t}};var j=class{_attachedPortal=null;_disposeFn=null;_isDisposed=!1;hasAttached(){return!!this._attachedPortal}attach(t){if(t instanceof at$1)return this._attachedPortal=t,this.attachComponentPortal(t);if(t instanceof M)return this._attachedPortal=t,this.attachTemplatePortal(t);if(this.attachDomPortal&&t instanceof lt$1)return this._attachedPortal=t,this.attachDomPortal(t)}attachDomPortal=null;detach(){this._attachedPortal&&(this._attachedPortal.setAttachedHost(null),this._attachedPortal=null),this._invokeDisposeFn()}dispose(){this.hasAttached()&&this.detach(),this._invokeDisposeFn(),this._isDisposed=!0}setDisposeFn(t){this._disposeFn=t}_invokeDisposeFn(){this._disposeFn&&(this._disposeFn(),this._disposeFn=null)}};var z=class extends j{outletElement;_appRef;_defaultInjector;constructor(t,e,i){super(),this.outletElement=t,this._appRef=e,this._defaultInjector=i}attachComponentPortal(t){let e;if(t.viewContainerRef){let i=t.injector||t.viewContainerRef.injector,s=i.get(bn,null,{optional:!0})||void 0;e=t.viewContainerRef.createComponent(t.component,{index:t.viewContainerRef.length,injector:i,ngModuleRef:s,projectableNodes:t.projectableNodes||void 0,bindings:t.bindings||void 0,directives:t.directives||void 0}),this.setDisposeFn(()=>e.destroy())}else{let i=this._appRef,s=t.injector||this._defaultInjector||ye$1.NULL,n=s.get(de$1,i.injector);e=i1(t.component,{elementInjector:s,environmentInjector:n,projectableNodes:t.projectableNodes||void 0,bindings:t.bindings||void 0,directives:t.directives||void 0}),i.attachView(e.hostView),this.setDisposeFn(()=>{i.viewCount>0&&i.detachView(e.hostView),e.destroy()})}return this.outletElement.appendChild(this._getComponentRootNode(e)),this._attachedPortal=t,e}attachTemplatePortal(t){let e=t.viewContainerRef,i=e.createEmbeddedView(t.templateRef,t.context,{injector:t.injector});return i.rootNodes.forEach(s=>this.outletElement.appendChild(s)),i.detectChanges(),this.setDisposeFn(()=>{let s=e.indexOf(i);s!==-1&&e.remove(s)}),this._attachedPortal=t,i}attachDomPortal=t=>{let e=t.element;e.parentNode;let i=this.outletElement.ownerDocument.createComment(`dom-portal`);e.parentNode.insertBefore(i,e),this.outletElement.appendChild(e),this._attachedPortal=t,super.setDisposeFn(()=>{i.parentNode&&i.parentNode.replaceChild(e,i)})};dispose(){super.dispose(),this.outletElement.remove()}_getComponentRootNode(t){return t.hostView.rootNodes[0]}};var pe$1=(()=>{class o extends j{_moduleRef=T(bn,{optional:!0});_document=T(ir);_viewContainerRef=T(Li$1);_isInitialized=!1;_attachedRef=null;get portal(){return this._attachedPortal}set portal(e){this.hasAttached()&&!e&&!this._isInitialized||(this.hasAttached()&&super.detach(),e&&super.attach(e),this._attachedPortal=e||null)}attached=new qe$1;get attachedRef(){return this._attachedRef}ngOnInit(){this._isInitialized=!0}ngOnDestroy(){super.dispose(),this._attachedRef=this._attachedPortal=null}attachComponentPortal(e){e.setAttachedHost(this);let i=e.viewContainerRef!=null?e.viewContainerRef:this._viewContainerRef,s=i.createComponent(e.component,{index:i.length,injector:e.injector||i.injector,projectableNodes:e.projectableNodes||void 0,ngModuleRef:this._moduleRef||void 0,bindings:e.bindings||void 0,directives:e.directives||void 0});return i!==this._viewContainerRef&&this._getRootNode().appendChild(s.hostView.rootNodes[0]),super.setDisposeFn(()=>s.destroy()),this._attachedPortal=e,this._attachedRef=s,this.attached.emit(s),s}attachTemplatePortal(e){e.setAttachedHost(this);let i=this._viewContainerRef.createEmbeddedView(e.templateRef,e.context,{injector:e.injector});return super.setDisposeFn(()=>this._viewContainerRef.clear()),this._attachedPortal=e,this._attachedRef=i,this.attached.emit(i),i}attachDomPortal=e=>{let i=e.element;i.parentNode;let s=this._document.createComment(`dom-portal`);e.setAttachedHost(this),i.parentNode.insertBefore(s,i),this._getRootNode().appendChild(i),this._attachedPortal=e,super.setDisposeFn(()=>{s.parentNode&&s.parentNode.replaceChild(i,s)})};_getRootNode(){let e=this._viewContainerRef.element.nativeElement;return e.nodeType===e.ELEMENT_NODE?e:e.parentNode}static ɵfac=(()=>{let e;return function(s){return(e||(e=iy(o)))(s||o)}})();static ɵdir=ME({type:o,selectors:[[``,`cdkPortalOutlet`,``]],inputs:{portal:[0,`cdkPortalOutlet`,`portal`]},outputs:{attached:`attached`},exportAs:[`cdkPortalOutlet`],features:[Op]})}return o})();var Vt=(()=>{class o{static ɵfac=function(i){return new(i||o)};static ɵmod=TE({type:o});static ɵinj=tu({})}return o})();var Nt$2=Zo();function Ht$1(o){return new Z$1(o.get(Qe$1),o.get(ir))}var Z$1=class{_viewportRuler;_previousHTMLStyles={top:``,left:``};_previousScrollPosition;_isEnabled=!1;_document;constructor(t,e){this._viewportRuler=t,this._document=e}attach(){}enable(){if(this._canBeEnabled()){let t=this._document.documentElement;this._previousScrollPosition=this._viewportRuler.getViewportScrollPosition(),this._previousHTMLStyles.left=t.style.left||``,this._previousHTMLStyles.top=t.style.top||``,t.style.left=ci$1(-this._previousScrollPosition.left),t.style.top=ci$1(-this._previousScrollPosition.top),t.classList.add(`cdk-global-scrollblock`),this._isEnabled=!0}}disable(){if(this._isEnabled){let t=this._document.documentElement,e=this._document.body,i=t.style,s=e.style,n=i.scrollBehavior||``,r=s.scrollBehavior||``;this._isEnabled=!1,i.left=this._previousHTMLStyles.left,i.top=this._previousHTMLStyles.top,t.classList.remove(`cdk-global-scrollblock`),Nt$2&&(i.scrollBehavior=s.scrollBehavior=`auto`),window.scroll(this._previousScrollPosition.left,this._previousScrollPosition.top),Nt$2&&(i.scrollBehavior=n,s.scrollBehavior=r)}}_canBeEnabled(){if(this._document.documentElement.classList.contains(`cdk-global-scrollblock`)||this._isEnabled)return!1;let e=this._document.documentElement,i=this._viewportRuler.getViewportSize();return e.scrollHeight>i.height||e.scrollWidth>i.width}};function Wt(o,t){return new U$1(o.get(N),o.get(Ae$2),o.get(Qe$1),t)}var U$1=class{_scrollDispatcher;_ngZone;_viewportRuler;_config;_scrollSubscription=null;_overlayRef;_initialScrollPosition;constructor(t,e,i,s){this._scrollDispatcher=t,this._ngZone=e,this._viewportRuler=i,this._config=s}attach(t){this._overlayRef,this._overlayRef=t}enable(){if(this._scrollSubscription)return;let t=this._scrollDispatcher.scrolled(0).pipe(Yt$1(e=>!e||!this._overlayRef.overlayElement.contains(e.getElementRef().nativeElement)));this._config&&this._config.threshold&&this._config.threshold>1?(this._initialScrollPosition=this._viewportRuler.getViewportScrollPosition().top,this._scrollSubscription=t.subscribe(()=>{let e=this._viewportRuler.getViewportScrollPosition().top;Math.abs(e-this._initialScrollPosition)>this._config.threshold?this._detach():this._overlayRef.updatePosition()})):this._scrollSubscription=t.subscribe(this._detach)}disable(){this._scrollSubscription&&(this._scrollSubscription.unsubscribe(),this._scrollSubscription=null)}detach(){this.disable(),this._overlayRef=null}_detach=()=>{this.disable(),this._overlayRef.hasAttached()&&this._ngZone.run(()=>this._overlayRef.detach())}};var A=class{enable(){}disable(){}attach(){}};function ht$1(o,t){return t.some(e=>{let i=o.bottom<e.top,s=o.top>e.bottom,n=o.right<e.left,r=o.left>e.right;return i||s||n||r})}function Ft$2(o,t){return t.some(e=>{let i=o.top<e.top,s=o.bottom>e.bottom,n=o.left<e.left,r=o.right>e.right;return i||s||n||r})}function pt$2(o,t){return new G$1(o.get(N),o.get(Qe$1),o.get(Ae$2),t)}var G$1=class{_scrollDispatcher;_viewportRuler;_ngZone;_config;_scrollSubscription=null;_overlayRef;constructor(t,e,i,s){this._scrollDispatcher=t,this._viewportRuler=e,this._ngZone=i,this._config=s}attach(t){this._overlayRef,this._overlayRef=t}enable(){if(!this._scrollSubscription){let t=this._config?this._config.scrollThrottle:0;this._scrollSubscription=this._scrollDispatcher.scrolled(t).subscribe(()=>{if(this._overlayRef.updatePosition(),this._config&&this._config.autoClose){let e=this._overlayRef.overlayElement.getBoundingClientRect(),{width:i,height:s}=this._viewportRuler.getViewportSize();ht$1(e,[{width:i,height:s,bottom:s,right:i,top:0,left:0}])&&(this.disable(),this._ngZone.run(()=>this._overlayRef.detach()))}})}}disable(){this._scrollSubscription&&(this._scrollSubscription.unsubscribe(),this._scrollSubscription=null)}detach(){this.disable(),this._overlayRef=null}};var jt$1=(()=>{class o{_injector=T(ye$1);noop=()=>new A;close=e=>Wt(this._injector,e);block=()=>Ht$1(this._injector);reposition=e=>pt$2(this._injector,e);static ɵfac=function(i){return new(i||o)};static ɵprov=Er({token:o,factory:o.ɵfac})}return o})();var B=class{positionStrategy;scrollStrategy=new A;panelClass=``;hasBackdrop=!1;backdropClass=`cdk-overlay-dark-backdrop`;disableAnimations;width;height;minWidth;minHeight;maxWidth;maxHeight;direction;disposeOnNavigation=!1;usePopover;eventPredicate;constructor(t){if(t){let e=Object.keys(t);for(let i of e)t[i]!==void 0&&(this[i]=t[i])}}};var K=class{connectionPair;scrollableViewProperties;constructor(t,e){this.connectionPair=t,this.scrollableViewProperties=e}};var zt$1=(()=>{class o{_attachedOverlays=[];_document=T(ir);_isAttached=!1;ngOnDestroy(){this.detach()}add(e){this.remove(e),this._attachedOverlays.push(e)}remove(e){let i=this._attachedOverlays.indexOf(e);i>-1&&this._attachedOverlays.splice(i,1),this._attachedOverlays.length===0&&this.detach()}canReceiveEvent(e,i,s){return s.observers.length<1?!1:e.eventPredicate?e.eventPredicate(i):!0}static ɵfac=function(i){return new(i||o)};static ɵprov=Er({token:o,factory:o.ɵfac})}return o})();var Zt=(()=>{class o extends zt$1{_ngZone=T(Ae$2);_renderer=T(mr).createRenderer(null,null);_cleanupKeydown;add(e){super.add(e),this._isAttached||(this._ngZone.runOutsideAngular(()=>{this._cleanupKeydown=this._renderer.listen(`body`,`keydown`,this._keydownListener)}),this._isAttached=!0)}detach(){this._isAttached&&(this._cleanupKeydown?.(),this._isAttached=!1)}_keydownListener=e=>{let i=this._attachedOverlays;for(let s=i.length-1;s>-1;s--){let n=i[s];if(this.canReceiveEvent(n,e,n._keydownEvents)){this._ngZone.run(()=>n._keydownEvents.next(e));break}}};static ɵfac=function(i){return new(i||o)};static ɵprov=Er({token:o,factory:o.ɵfac})}return o})();var Ut=(()=>{class o extends zt$1{_platform=T(l$1);_ngZone=T(Ae$2);_renderer=T(mr).createRenderer(null,null);_cursorOriginalValue;_cursorStyleIsSet=!1;_pointerDownEventTarget=null;_cleanups;add(e){if(super.add(e),!this._isAttached){let i=this._document.body,s={capture:!0},n=this._renderer;this._cleanups=this._ngZone.runOutsideAngular(()=>[n.listen(i,`pointerdown`,this._pointerDownListener,s),n.listen(i,`click`,this._clickListener,s),n.listen(i,`auxclick`,this._clickListener,s),n.listen(i,`contextmenu`,this._clickListener,s)]),this._platform.IOS&&!this._cursorStyleIsSet&&(this._cursorOriginalValue=i.style.cursor,i.style.cursor=`pointer`,this._cursorStyleIsSet=!0),this._isAttached=!0}}detach(){this._isAttached&&(this._cleanups?.forEach(e=>e()),this._cleanups=void 0,this._platform.IOS&&this._cursorStyleIsSet&&(this._document.body.style.cursor=this._cursorOriginalValue,this._cursorStyleIsSet=!1),this._isAttached=!1)}_pointerDownListener=e=>{this._pointerDownEventTarget=y$1(e)};_clickListener=e=>{let i=y$1(e),s=e.type===`click`&&this._pointerDownEventTarget?this._pointerDownEventTarget:i;this._pointerDownEventTarget=null;let n=this._attachedOverlays.slice();for(let r=n.length-1;r>-1;r--){let a=n[r],h=a._outsidePointerEvents;if(!(!a.hasAttached()||!this.canReceiveEvent(a,e,h))){if(Yt(a.overlayElement,i)||Yt(a.overlayElement,s))break;this._ngZone?this._ngZone.run(()=>h.next(e)):h.next(e)}}};static ɵfac=function(i){return new(i||o)};static ɵprov=Er({token:o,factory:o.ɵfac})}return o})();function Yt(o,t){let e=typeof ShadowRoot<`u`&&ShadowRoot,i=t;for(;i;){if(i===o)return!0;i=e&&i instanceof ShadowRoot?i.host:i.parentNode}return!1}var Gt=(()=>{class o{static ɵfac=function(i){return new(i||o)};static ɵcmp=wE({type:o,selectors:[[`ng-component`]],hostAttrs:[`cdk-overlay-style-loader`,``],decls:0,vars:0,template:function(i,s){},styles:[`.cdk-overlay-container, .cdk-global-overlay-wrapper {
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
`],encapsulation:2})}return o})();var Kt=(()=>{class o{_platform=T(l$1);_containerElement;_document=T(ir);_styleLoader=T(M$2);ngOnDestroy(){this._containerElement?.remove()}getContainerElement(){return this._loadStyles(),this._containerElement||this._createContainer(),this._containerElement}_createContainer(){let e=`cdk-overlay-container`;if(this._platform.isBrowser||Yo()){let s=this._document.querySelectorAll(`.${e}[platform="server"], .${e}[platform="test"]`);for(let n=0;n<s.length;n++)s[n].remove()}let i=this._document.createElement(`div`);i.classList.add(e),Yo()?i.setAttribute(`platform`,`test`):this._platform.isBrowser||i.setAttribute(`platform`,`server`),this._document.body.appendChild(i),this._containerElement=i}_loadStyles(){this._styleLoader.load(Gt)}static ɵfac=function(i){return new(i||o)};static ɵprov=Er({token:o,factory:o.ɵfac})}return o})();var ct$1=class{_renderer;_ngZone;element;_cleanupClick;_cleanupTransitionEnd;_fallbackTimeout;constructor(t,e,i,s){this._renderer=e,this._ngZone=i,this.element=t.createElement(`div`),this.element.classList.add(`cdk-overlay-backdrop`),this._cleanupClick=e.listen(this.element,`click`,s)}detach(){this._ngZone.runOutsideAngular(()=>{let t=this.element;clearTimeout(this._fallbackTimeout),this._cleanupTransitionEnd?.(),this._cleanupTransitionEnd=this._renderer.listen(t,`transitionend`,this.dispose),this._fallbackTimeout=setTimeout(this.dispose,500),t.style.pointerEvents=`none`,t.classList.remove(`cdk-overlay-backdrop-showing`)})}dispose=()=>{clearTimeout(this._fallbackTimeout),this._cleanupClick?.(),this._cleanupTransitionEnd?.(),this._cleanupClick=this._cleanupTransitionEnd=this._fallbackTimeout=void 0,this.element.remove()}};function ft$2(o){return o&&o.nodeType===1}var $$2=class{_portalOutlet;_host;_pane;_config;_ngZone;_keyboardDispatcher;_document;_location;_outsideClickDispatcher;_animationsDisabled;_injector;_renderer;_backdropClick=new Q$1;_attachments=new Q$1;_detachments=new Q$1;_positionStrategy;_scrollStrategy;_locationChanges=j$1.EMPTY;_backdropRef=null;_detachContentMutationObserver;_detachContentAfterRenderRef;_disposed=!1;_previousHostParent;_keydownEvents=new Q$1;_outsidePointerEvents=new Q$1;_afterNextRenderRef;constructor(t,e,i,s,n,r,a,h,p,l=!1,d,u){this._portalOutlet=t,this._host=e,this._pane=i,this._config=s,this._ngZone=n,this._keyboardDispatcher=r,this._document=a,this._location=h,this._outsideClickDispatcher=p,this._animationsDisabled=l,this._injector=d,this._renderer=u,s.scrollStrategy&&(this._scrollStrategy=s.scrollStrategy,this._scrollStrategy.attach(this)),this._positionStrategy=s.positionStrategy}get overlayElement(){return this._pane}get backdropElement(){return this._backdropRef?.element||null}get hostElement(){return this._host}get eventPredicate(){return this._config?.eventPredicate||null}attach(t){if(this._disposed)return null;this._attachHost();let e=this._portalOutlet.attach(t);return this._positionStrategy?.attach(this),this._updateStackingOrder(),this._updateElementSize(),this._updateElementDirection(),this._scrollStrategy&&this._scrollStrategy.enable(),this._afterNextRenderRef?.destroy(),this._afterNextRenderRef=Dv(()=>{this.hasAttached()&&this.updatePosition()},{injector:this._injector}),this._togglePointerEvents(!0),this._config.hasBackdrop&&this._attachBackdrop(),this._config.panelClass&&this._toggleClasses(this._pane,this._config.panelClass,!0),this._attachments.next(),this._completeDetachContent(),this._keyboardDispatcher.add(this),this._config.disposeOnNavigation&&(this._locationChanges=this._location.subscribe(()=>this.dispose())),this._outsideClickDispatcher.add(this),typeof e?.onDestroy==`function`&&e.onDestroy(()=>{this.hasAttached()&&this._ngZone.runOutsideAngular(()=>Promise.resolve().then(()=>this.detach()))}),e}detach(){if(!this.hasAttached())return;this.detachBackdrop(),this._togglePointerEvents(!1),this._positionStrategy&&this._positionStrategy.detach&&this._positionStrategy.detach(),this._scrollStrategy&&this._scrollStrategy.disable();let t=this._portalOutlet.detach();return this._detachments.next(),this._completeDetachContent(),this._keyboardDispatcher.remove(this),this._detachContentWhenEmpty(),this._locationChanges.unsubscribe(),this._outsideClickDispatcher.remove(this),t}dispose(){if(this._disposed)return;let t=this.hasAttached();this._positionStrategy&&this._positionStrategy.dispose(),this._disposeScrollStrategy(),this._backdropRef?.dispose(),this._locationChanges.unsubscribe(),this._keyboardDispatcher.remove(this),this._portalOutlet.dispose(),this._attachments.complete(),this._backdropClick.complete(),this._keydownEvents.complete(),this._outsidePointerEvents.complete(),this._outsideClickDispatcher.remove(this),this._host?.remove(),this._afterNextRenderRef?.destroy(),this._previousHostParent=this._pane=this._host=this._backdropRef=null,t&&this._detachments.next(),this._detachments.complete(),this._completeDetachContent(),this._disposed=!0}hasAttached(){return this._portalOutlet.hasAttached()}backdropClick(){return this._backdropClick}attachments(){return this._attachments}detachments(){return this._detachments}keydownEvents(){return this._keydownEvents}outsidePointerEvents(){return this._outsidePointerEvents}getConfig(){return this._config}updatePosition(){this._positionStrategy&&this._positionStrategy.apply()}updatePositionStrategy(t){t!==this._positionStrategy&&(this._positionStrategy&&this._positionStrategy.dispose(),this._positionStrategy=t,this.hasAttached()&&(t.attach(this),this.updatePosition()))}updateSize(t){this._config=r(r({},this._config),t),this._updateElementSize()}setDirection(t){this._config=s(r({},this._config),{direction:t}),this._updateElementDirection()}addPanelClass(t){this._pane&&this._toggleClasses(this._pane,t,!0)}removePanelClass(t){this._pane&&this._toggleClasses(this._pane,t,!1)}getDirection(){let t=this._config.direction;return t?typeof t==`string`?t:t.value:`ltr`}updateScrollStrategy(t){t!==this._scrollStrategy&&(this._disposeScrollStrategy(),this._scrollStrategy=t,this.hasAttached()&&(t.attach(this),t.enable()))}_updateElementDirection(){this._host.setAttribute(`dir`,this.getDirection())}_updateElementSize(){if(!this._pane)return;let t=this._pane.style;t.width=ci$1(this._config.width),t.height=ci$1(this._config.height),t.minWidth=ci$1(this._config.minWidth),t.minHeight=ci$1(this._config.minHeight),t.maxWidth=ci$1(this._config.maxWidth),t.maxHeight=ci$1(this._config.maxHeight)}_togglePointerEvents(t){this._pane.style.pointerEvents=t?``:`none`}_attachHost(){if(!this._host.parentElement){let t=this._config.usePopover?this._positionStrategy?.getPopoverInsertionPoint?.():null;ft$2(t)?t.after(this._host):t?.type===`parent`?t.element.appendChild(this._host):this._previousHostParent?.appendChild(this._host)}if(this._config.usePopover)try{this._host.showPopover()}catch{}}_attachBackdrop(){let t=`cdk-overlay-backdrop-showing`;this._backdropRef?.dispose(),this._backdropRef=new ct$1(this._document,this._renderer,this._ngZone,e=>{this._backdropClick.next(e)}),this._animationsDisabled&&this._backdropRef.element.classList.add(`cdk-overlay-backdrop-noop-animation`),this._config.backdropClass&&this._toggleClasses(this._backdropRef.element,this._config.backdropClass,!0),this._config.usePopover?this._host.prepend(this._backdropRef.element):this._host.parentElement.insertBefore(this._backdropRef.element,this._host),!this._animationsDisabled&&typeof requestAnimationFrame<`u`?this._ngZone.runOutsideAngular(()=>{requestAnimationFrame(()=>this._backdropRef?.element.classList.add(t))}):this._backdropRef.element.classList.add(t)}_updateStackingOrder(){!this._config.usePopover&&this._host.nextSibling&&this._host.parentNode.appendChild(this._host)}detachBackdrop(){this._animationsDisabled?(this._backdropRef?.dispose(),this._backdropRef=null):this._backdropRef?.detach()}_toggleClasses(t,e,i){let s=Ot$1(e||[]).filter(n=>!!n);s.length&&(i?t.classList.add(...s):t.classList.remove(...s))}_detachContentWhenEmpty(){let t=!1;try{this._detachContentAfterRenderRef=Dv(()=>{t=!0,this._detachContent()},{injector:this._injector})}catch(e){if(t)throw e;this._detachContent()}globalThis.MutationObserver&&this._pane&&(this._detachContentMutationObserver||=new globalThis.MutationObserver(()=>{this._detachContent()}),this._detachContentMutationObserver.observe(this._pane,{childList:!0}))}_detachContent(){(!this._pane||!this._host||this._pane.children.length===0)&&(this._pane&&this._config.panelClass&&this._toggleClasses(this._pane,this._config.panelClass,!1),this._host&&this._host.parentElement&&(this._previousHostParent=this._host.parentElement,this._host.remove()),this._completeDetachContent())}_completeDetachContent(){this._detachContentAfterRenderRef?.destroy(),this._detachContentAfterRenderRef=void 0,this._detachContentMutationObserver?.disconnect()}_disposeScrollStrategy(){let t=this._scrollStrategy;t?.disable(),t?.detach?.()}};var Tt=`cdk-overlay-connected-position-bounding-box`;var ee=/([A-Za-z%]+)$/;function ut$2(o,t){return new q(t,o.get(Qe$1),o.get(ir),o.get(l$1),o.get(Kt))}var q=class{_viewportRuler;_document;_platform;_overlayContainer;_overlayRef;_isInitialRender=!1;_lastBoundingBoxSize={width:0,height:0};_isPushed=!1;_canPush=!0;_growAfterOpen=!1;_hasFlexibleDimensions=!0;_positionLocked=!1;_originRect;_overlayRect;_viewportRect;_containerRect;_viewportMargin=0;_scrollables=[];_preferredPositions=[];_origin;_pane;_isDisposed=!1;_boundingBox=null;_lastPosition=null;_lastScrollVisibility=null;_positionChanges=new Q$1;_resizeSubscription=j$1.EMPTY;_offsetX=0;_offsetY=0;_transformOriginSelector;_appliedPanelClasses=[];_previousPushAmount=null;_popoverLocation=`global`;positionChanges=this._positionChanges;get positions(){return this._preferredPositions}constructor(t,e,i,s,n){this._viewportRuler=e,this._document=i,this._platform=s,this._overlayContainer=n,this.setOrigin(t)}attach(t){this._overlayRef&&this._overlayRef,this._validatePositions(),t.hostElement.classList.add(Tt),this._overlayRef=t,this._boundingBox=t.hostElement,this._pane=t.overlayElement,this._isDisposed=!1,this._isInitialRender=!0,this._lastPosition=null,this._resizeSubscription.unsubscribe(),this._resizeSubscription=this._viewportRuler.change().subscribe(()=>{this._isInitialRender=!0,this.apply()})}apply(){if(this._isDisposed||!this._platform.isBrowser)return;if(!this._isInitialRender&&this._positionLocked&&this._lastPosition){this.reapplyLastPosition();return}this._clearPanelClasses(),this._resetOverlayElementStyles(),this._resetBoundingBoxStyles(),this._viewportRect=this._getNarrowedViewportRect(),this._originRect=this._getOriginRect(),this._overlayRect=this._pane.getBoundingClientRect(),this._containerRect=this._getContainerRect();let t=this._originRect,e=this._overlayRect,i=this._viewportRect,s=this._containerRect,n=[],r;for(let a of this._preferredPositions){let h=this._getOriginPoint(t,s,a),p=this._getOverlayPoint(h,e,a),l=this._getOverlayFit(p,e,i,a);if(l.isCompletelyWithinViewport){this._isPushed=!1,this._applyPosition(a,h);return}if(this._canFitWithFlexibleDimensions(l,p,i)){n.push({position:a,origin:h,overlayRect:e,boundingBoxRect:this._calculateBoundingBoxRect(h,a)});continue}(!r||r.overlayFit.visibleArea<l.visibleArea)&&(r={overlayFit:l,overlayPoint:p,originPoint:h,position:a,overlayRect:e})}if(n.length){let a=null,h=-1;for(let p of n){let l=p.boundingBoxRect.width*p.boundingBoxRect.height*(p.position.weight||1);l>h&&(h=l,a=p)}this._isPushed=!1,this._applyPosition(a.position,a.origin);return}if(this._canPush){this._isPushed=!0,this._applyPosition(r.position,r.originPoint);return}this._applyPosition(r.position,r.originPoint)}detach(){this._clearPanelClasses(),this._lastPosition=null,this._previousPushAmount=null,this._resizeSubscription.unsubscribe()}dispose(){this._isDisposed||(this._boundingBox&&R(this._boundingBox.style,{top:``,left:``,right:``,bottom:``,height:``,width:``,alignItems:``,justifyContent:``}),this._pane&&this._resetOverlayElementStyles(),this._overlayRef&&this._overlayRef.hostElement.classList.remove(Tt),this.detach(),this._positionChanges.complete(),this._overlayRef=this._boundingBox=null,this._isDisposed=!0)}reapplyLastPosition(){if(this._isDisposed||!this._platform.isBrowser)return;let t=this._lastPosition;t?(this._originRect=this._getOriginRect(),this._overlayRect=this._pane.getBoundingClientRect(),this._viewportRect=this._getNarrowedViewportRect(),this._containerRect=this._getContainerRect(),this._applyPosition(t,this._getOriginPoint(this._originRect,this._containerRect,t))):this.apply()}withScrollableContainers(t){return this._scrollables=t,this}withPositions(t){return this._preferredPositions=t,t.indexOf(this._lastPosition)===-1&&(this._lastPosition=null),this._validatePositions(),this}withViewportMargin(t){return this._viewportMargin=t,this}withFlexibleDimensions(t=!0){return this._hasFlexibleDimensions=t,this}withGrowAfterOpen(t=!0){return this._growAfterOpen=t,this}withPush(t=!0){return this._canPush=t,this}withLockedPosition(t=!0){return this._positionLocked=t,this}setOrigin(t){return this._origin=t,this}withDefaultOffsetX(t){return this._offsetX=t,this}withDefaultOffsetY(t){return this._offsetY=t,this}withTransformOriginOn(t){return this._transformOriginSelector=t,this}withPopoverLocation(t){return this._popoverLocation=t,this}getPopoverInsertionPoint(){return this._popoverLocation===`global`?null:this._popoverLocation!==`inline`?this._popoverLocation:this._origin instanceof Dr?this._origin.nativeElement:ft$2(this._origin)?this._origin:null}_getOriginPoint(t,e,i){let s;if(i.originX==`center`)s=t.left+t.width/2;else{let r=this._isRtl()?t.right:t.left,a=this._isRtl()?t.left:t.right;s=i.originX==`start`?r:a}e.left<0&&(s-=e.left);let n;return i.originY==`center`?n=t.top+t.height/2:n=i.originY==`top`?t.top:t.bottom,e.top<0&&(n-=e.top),{x:s,y:n}}_getOverlayPoint(t,e,i){let s;i.overlayX==`center`?s=-e.width/2:i.overlayX===`start`?s=this._isRtl()?-e.width:0:s=this._isRtl()?0:-e.width;let n;return i.overlayY==`center`?n=-e.height/2:n=i.overlayY==`top`?0:-e.height,{x:t.x+s,y:t.y+n}}_getOverlayFit(t,e,i,s){let n=Xt(e),{x:r,y:a}=t,h=this._getOffset(s,`x`),p=this._getOffset(s,`y`);h&&(r+=h),p&&(a+=p);let l=0-r,d=r+n.width-i.width,u=0-a,g=a+n.height-i.height,_=this._subtractOverflows(n.width,l,d),v=this._subtractOverflows(n.height,u,g),vt=_*v;return{visibleArea:vt,isCompletelyWithinViewport:n.width*n.height===vt,fitsInViewportVertically:v===n.height,fitsInViewportHorizontally:_==n.width}}_canFitWithFlexibleDimensions(t,e,i){if(this._hasFlexibleDimensions){let s=i.bottom-e.y,n=i.right-e.x,r=Lt$1(this._overlayRef.getConfig().minHeight),a=Lt$1(this._overlayRef.getConfig().minWidth),h=t.fitsInViewportVertically||r!=null&&r<=s,p=t.fitsInViewportHorizontally||a!=null&&a<=n;return h&&p}return!1}_pushOverlayOnScreen(t,e,i){if(this._previousPushAmount&&this._positionLocked)return{x:t.x+this._previousPushAmount.x,y:t.y+this._previousPushAmount.y};let s=Xt(e),n=this._viewportRect,r=Math.max(t.x+s.width-n.width,0),a=Math.max(t.y+s.height-n.height,0),h=Math.max(n.top-i.top-t.y,0),p=Math.max(n.left-i.left-t.x,0),l=0,d=0;return s.width<=n.width?l=p||-r:l=t.x<this._getViewportMarginStart()?n.left-i.left-t.x:0,s.height<=n.height?d=h||-a:d=t.y<this._getViewportMarginTop()?n.top-i.top-t.y:0,this._previousPushAmount={x:l,y:d},{x:t.x+l,y:t.y+d}}_applyPosition(t,e){if(this._setTransformOrigin(t),this._setOverlayElementStyles(e,t),this._setBoundingBoxStyles(e,t),t.panelClass&&this._addPanelClasses(t.panelClass),this._positionChanges.observers.length){let i=this._getScrollVisibility();if(t!==this._lastPosition||!this._lastScrollVisibility||!ie$1(this._lastScrollVisibility,i)){let s=new K(t,i);this._positionChanges.next(s)}this._lastScrollVisibility=i}this._lastPosition=t,this._isInitialRender=!1}_setTransformOrigin(t){if(!this._transformOriginSelector)return;let e=this._boundingBox.querySelectorAll(this._transformOriginSelector),i,s=t.overlayY;t.overlayX===`center`?i=`center`:this._isRtl()?i=t.overlayX===`start`?`right`:`left`:i=t.overlayX===`start`?`left`:`right`;for(let n=0;n<e.length;n++)e[n].style.transformOrigin=`${i} ${s}`}_calculateBoundingBoxRect(t,e){let i=this._viewportRect,s=this._isRtl(),n,r,a;if(e.overlayY===`top`)r=t.y,n=i.height-r+this._getViewportMarginBottom();else if(e.overlayY===`bottom`)a=i.height-t.y+this._getViewportMarginTop()+this._getViewportMarginBottom(),n=i.height-a+this._getViewportMarginTop();else{let g=Math.min(i.bottom-t.y+i.top,t.y),_=this._lastBoundingBoxSize.height;n=g*2,r=t.y-g,n>_&&!this._isInitialRender&&!this._growAfterOpen&&(r=t.y-_/2)}let h=e.overlayX===`start`&&!s||e.overlayX===`end`&&s,p=e.overlayX===`end`&&!s||e.overlayX===`start`&&s,l,d,u;if(p)u=i.width-t.x+this._getViewportMarginStart()+this._getViewportMarginEnd(),l=t.x-this._getViewportMarginStart();else if(h)d=t.x,l=i.right-t.x-this._getViewportMarginEnd();else{let g=Math.min(i.right-t.x+i.left,t.x),_=this._lastBoundingBoxSize.width;l=g*2,d=t.x-g,l>_&&!this._isInitialRender&&!this._growAfterOpen&&(d=t.x-_/2)}return{top:r,left:d,bottom:a,right:u,width:l,height:n}}_setBoundingBoxStyles(t,e){let i=this._calculateBoundingBoxRect(t,e);!this._isInitialRender&&!this._growAfterOpen&&(i.height=Math.min(i.height,this._lastBoundingBoxSize.height),i.width=Math.min(i.width,this._lastBoundingBoxSize.width));let s={};if(this._hasExactPosition())s.top=s.left=`0`,s.bottom=s.right=`auto`,s.maxHeight=s.maxWidth=``,s.width=s.height=`100%`;else{let n=this._overlayRef.getConfig().maxHeight,r=this._overlayRef.getConfig().maxWidth;s.width=ci$1(i.width),s.height=ci$1(i.height),s.top=ci$1(i.top)||`auto`,s.bottom=ci$1(i.bottom)||`auto`,s.left=ci$1(i.left)||`auto`,s.right=ci$1(i.right)||`auto`,e.overlayX===`center`?s.alignItems=`center`:s.alignItems=e.overlayX===`end`?`flex-end`:`flex-start`,e.overlayY===`center`?s.justifyContent=`center`:s.justifyContent=e.overlayY===`bottom`?`flex-end`:`flex-start`,n&&(s.maxHeight=ci$1(n)),r&&(s.maxWidth=ci$1(r))}this._lastBoundingBoxSize=i,R(this._boundingBox.style,s)}_resetBoundingBoxStyles(){R(this._boundingBox.style,{top:`0`,left:`0`,right:`0`,bottom:`0`,height:``,width:``,alignItems:``,justifyContent:``})}_resetOverlayElementStyles(){R(this._pane.style,{top:``,left:``,bottom:``,right:``,position:``,transform:``})}_setOverlayElementStyles(t,e){let i={},s=this._hasExactPosition(),n=this._hasFlexibleDimensions,r=this._overlayRef.getConfig();if(s){let l=this._viewportRuler.getViewportScrollPosition();R(i,this._getExactOverlayY(e,t,l)),R(i,this._getExactOverlayX(e,t,l))}else i.position=`static`;let a=``,h=this._getOffset(e,`x`),p=this._getOffset(e,`y`);h&&(a+=`translateX(${h}px) `),p&&(a+=`translateY(${p}px)`),i.transform=a.trim(),r.maxHeight&&(s?i.maxHeight=ci$1(r.maxHeight):n&&(i.maxHeight=``)),r.maxWidth&&(s?i.maxWidth=ci$1(r.maxWidth):n&&(i.maxWidth=``)),R(this._pane.style,i)}_getExactOverlayY(t,e,i){let s={top:``,bottom:``},n=this._getOverlayPoint(e,this._overlayRect,t);if(this._isPushed&&(n=this._pushOverlayOnScreen(n,this._overlayRect,i)),t.overlayY===`bottom`)s.bottom=`${this._document.documentElement.clientHeight-(n.y+this._overlayRect.height)}px`;else s.top=ci$1(n.y);return s}_getExactOverlayX(t,e,i){let s={left:``,right:``},n=this._getOverlayPoint(e,this._overlayRect,t);this._isPushed&&(n=this._pushOverlayOnScreen(n,this._overlayRect,i));let r;if(this._isRtl()?r=t.overlayX===`end`?`left`:`right`:r=t.overlayX===`end`?`right`:`left`,r===`right`)s.right=`${this._document.documentElement.clientWidth-(n.x+this._overlayRect.width)}px`;else s.left=ci$1(n.x);return s}_getScrollVisibility(){let t=this._getOriginRect(),e=this._pane.getBoundingClientRect(),i=this._scrollables.map(s=>s.getElementRef().nativeElement.getBoundingClientRect());return{isOriginClipped:Ft$2(t,i),isOriginOutsideView:ht$1(t,i),isOverlayClipped:Ft$2(e,i),isOverlayOutsideView:ht$1(e,i)}}_subtractOverflows(t,...e){return e.reduce((i,s)=>i-Math.max(s,0),t)}_getNarrowedViewportRect(){let t=this._document.documentElement.clientWidth,e=this._document.documentElement.clientHeight,i=this._viewportRuler.getViewportScrollPosition();return{top:i.top+this._getViewportMarginTop(),left:i.left+this._getViewportMarginStart(),right:i.left+t-this._getViewportMarginEnd(),bottom:i.top+e-this._getViewportMarginBottom(),width:t-this._getViewportMarginStart()-this._getViewportMarginEnd(),height:e-this._getViewportMarginTop()-this._getViewportMarginBottom()}}_isRtl(){return this._overlayRef.getDirection()===`rtl`}_hasExactPosition(){return!this._hasFlexibleDimensions||this._isPushed}_getOffset(t,e){return e===`x`?t.offsetX==null?this._offsetX:t.offsetX:t.offsetY==null?this._offsetY:t.offsetY}_validatePositions(){}_addPanelClasses(t){this._pane&&Ot$1(t).forEach(e=>{e!==``&&this._appliedPanelClasses.indexOf(e)===-1&&(this._appliedPanelClasses.push(e),this._pane.classList.add(e))})}_clearPanelClasses(){this._pane&&(this._appliedPanelClasses.forEach(t=>{this._pane.classList.remove(t)}),this._appliedPanelClasses=[])}_getViewportMarginStart(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.start??0}_getViewportMarginEnd(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.end??0}_getViewportMarginTop(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.top??0}_getViewportMarginBottom(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.bottom??0}_getOriginRect(){let t=this._origin;if(t instanceof Dr)return t.nativeElement.getBoundingClientRect();if(t instanceof Element)return t.getBoundingClientRect();let e=t.width||0,i=t.height||0;return{top:t.y,bottom:t.y+i,left:t.x,right:t.x+e,height:i,width:e}}_getContainerRect(){let t=this._overlayRef.getConfig().usePopover&&this._popoverLocation!==`global`,e=this._overlayContainer.getContainerElement();t&&(e.style.display=`block`);let i=e.getBoundingClientRect();return t&&(e.style.display=``),i}};function R(o,t){for(let e in t)t.hasOwnProperty(e)&&(o[e]=t[e]);return o}function Lt$1(o){if(typeof o!=`number`&&o!=null){let[t,e]=o.split(ee);return!e||e===`px`?parseFloat(t):null}return o||null}function Xt(o){return{top:Math.floor(o.top),right:Math.floor(o.right),bottom:Math.floor(o.bottom),left:Math.floor(o.left),width:Math.floor(o.width),height:Math.floor(o.height)}}function ie$1(o,t){return o===t?!0:o.isOriginClipped===t.isOriginClipped&&o.isOriginOutsideView===t.isOriginOutsideView&&o.isOverlayClipped===t.isOverlayClipped&&o.isOverlayOutsideView===t.isOverlayOutsideView}var It=`cdk-global-overlay-wrapper`;function $t(o){return new J}var J=class{_overlayRef;_cssPosition=`static`;_topOffset=``;_bottomOffset=``;_alignItems=``;_xPosition=``;_xOffset=``;_width=``;_height=``;_isDisposed=!1;attach(t){let e=t.getConfig();this._overlayRef=t,this._width&&!e.width&&t.updateSize({width:this._width}),this._height&&!e.height&&t.updateSize({height:this._height}),t.hostElement.classList.add(It),this._isDisposed=!1}top(t=``){return this._bottomOffset=``,this._topOffset=t,this._alignItems=`flex-start`,this}left(t=``){return this._xOffset=t,this._xPosition=`left`,this}bottom(t=``){return this._topOffset=``,this._bottomOffset=t,this._alignItems=`flex-end`,this}right(t=``){return this._xOffset=t,this._xPosition=`right`,this}start(t=``){return this._xOffset=t,this._xPosition=`start`,this}end(t=``){return this._xOffset=t,this._xPosition=`end`,this}width(t=``){return this._overlayRef?this._overlayRef.updateSize({width:t}):this._width=t,this}height(t=``){return this._overlayRef?this._overlayRef.updateSize({height:t}):this._height=t,this}centerHorizontally(t=``){return this.left(t),this._xPosition=`center`,this}centerVertically(t=``){return this.top(t),this._alignItems=`center`,this}apply(){if(!this._overlayRef||!this._overlayRef.hasAttached())return;let t=this._overlayRef.overlayElement.style,e=this._overlayRef.hostElement.style,{width:s,height:n,maxWidth:r,maxHeight:a}=this._overlayRef.getConfig(),h=(s===`100%`||s===`100vw`)&&(!r||r===`100%`||r===`100vw`),p=(n===`100%`||n===`100vh`)&&(!a||a===`100%`||a===`100vh`),l=this._xPosition,d=this._xOffset,u=this._overlayRef.getConfig().direction===`rtl`,g=``,_=``,v=``;h?v=`flex-start`:l===`center`?(v=`center`,u?_=d:g=d):u?l===`left`||l===`end`?(v=`flex-end`,g=d):(l===`right`||l===`start`)&&(v=`flex-start`,_=d):l===`left`||l===`start`?(v=`flex-start`,g=d):(l===`right`||l===`end`)&&(v=`flex-end`,_=d),t.position=this._cssPosition,t.marginLeft=h?`0`:g,t.marginTop=p?`0`:this._topOffset,t.marginBottom=this._bottomOffset,t.marginRight=h?`0`:_,e.justifyContent=v,e.alignItems=p?`flex-start`:this._alignItems}dispose(){if(this._isDisposed||!this._overlayRef)return;let t=this._overlayRef.overlayElement.style,e=this._overlayRef.hostElement,i=e.style;e.classList.remove(It),i.justifyContent=i.alignItems=t.marginTop=t.marginBottom=t.marginLeft=t.marginRight=t.position=``,this._overlayRef=null,this._isDisposed=!0}};var qt=(()=>{class o{_injector=T(ye$1);global(){return $t()}flexibleConnectedTo(e){return ut$2(this._injector,e)}static ɵfac=function(i){return new(i||o)};static ɵprov=Er({token:o,factory:o.ɵfac})}return o})();var _t$1=new A$2(`OVERLAY_DEFAULT_CONFIG`);function gt$1(o,t){o.get(M$2).load(Gt);let e=o.get(Kt),i=o.get(ir),s=o.get(Bt$1),n=o.get(Pi$1),r=o.get(Yn$1),a=o.get(Wa,null,{optional:!0})||o.get(mr).createRenderer(null,null),h=new B(t),p=o.get(_t$1,null,{optional:!0})?.usePopover??!0;h.direction=h.direction||r.value,!i.body||!(`showPopover`in i.body)?h.usePopover=!1:h.usePopover=t?.usePopover??p;let l=i.createElement(`div`),d=i.createElement(`div`);l.id=s.getId(`cdk-overlay-`),l.classList.add(`cdk-overlay-pane`),d.appendChild(l),h.usePopover&&(d.setAttribute(`popover`,`manual`),d.classList.add(`cdk-overlay-popover`));let u=h.usePopover?h.positionStrategy?.getPopoverInsertionPoint?.():null;return ft$2(u)?u.after(d):u?.type===`parent`?u.element.appendChild(d):e.getContainerElement().appendChild(d),new $$2(new z(l,n,o),d,l,h,o.get(Ae$2),o.get(Zt),i,o.get(Ze),o.get(Ut),t?.disableAnimations??o.get(Cm,null,{optional:!0})===`NoopAnimations`,o.get(de$1),a)}var Jt=(()=>{class o{scrollStrategies=T(jt$1);_positionBuilder=T(qt);_injector=T(ye$1);create(e){return gt$1(this._injector,e)}position(){return this._positionBuilder}static ɵfac=function(i){return new(i||o)};static ɵprov=Er({token:o,factory:o.ɵfac})}return o})();var oe$1=[{originX:`start`,originY:`bottom`,overlayX:`start`,overlayY:`top`},{originX:`start`,originY:`top`,overlayX:`start`,overlayY:`bottom`},{originX:`end`,originY:`top`,overlayX:`end`,overlayY:`bottom`},{originX:`end`,originY:`bottom`,overlayX:`end`,overlayY:`top`}];var se$1=new A$2(`cdk-connected-overlay-scroll-strategy`,{providedIn:`root`,factory:()=>{let o=T(ye$1);return()=>pt$2(o)}});var dt$1=(()=>{class o{elementRef=T(Dr);static ɵfac=function(i){return new(i||o)};static ɵdir=ME({type:o,selectors:[[``,`cdk-overlay-origin`,``],[``,`overlay-origin`,``],[``,`cdkOverlayOrigin`,``]],exportAs:[`cdkOverlayOrigin`]})}return o})();var Qt=new A$2(`cdk-connected-overlay-default-config`);var ne$1=(()=>{class o{_dir=T(Yn$1,{optional:!0});_injector=T(ye$1);_overlayRef;_templatePortal;_backdropSubscription=j$1.EMPTY;_attachSubscription=j$1.EMPTY;_detachSubscription=j$1.EMPTY;_positionSubscription=j$1.EMPTY;_offsetX;_offsetY;_position;_scrollStrategyFactory=T(se$1);_ngZone=T(Ae$2);origin;positions;positionStrategy;get offsetX(){return this._offsetX}set offsetX(e){this._offsetX=e,this._position&&this._updatePositionStrategy(this._position)}get offsetY(){return this._offsetY}set offsetY(e){this._offsetY=e,this._position&&this._updatePositionStrategy(this._position)}width;height;minWidth;minHeight;backdropClass;panelClass;viewportMargin=0;scrollStrategy;open=!1;disableClose=!1;transformOriginSelector;hasBackdrop=!1;lockPosition=!1;flexibleDimensions=!1;growAfterOpen=!1;push=!1;disposeOnNavigation=!1;usePopover;matchWidth=!1;set _config(e){typeof e!=`string`&&this._assignConfig(e)}backdropClick=new qe$1;positionChange=new qe$1;attach=new qe$1;detach=new qe$1;overlayKeydown=new qe$1;overlayOutsideClick=new qe$1;constructor(){let e=T(gr),i=T(Li$1),s=T(Qt,{optional:!0}),n=T(_t$1,{optional:!0});this.usePopover=n?.usePopover===!1?null:`global`,this._templatePortal=new M(e,i),this.scrollStrategy=this._scrollStrategyFactory(),s&&this._assignConfig(s)}get overlayRef(){return this._overlayRef}get dir(){return this._dir?this._dir.value:`ltr`}ngOnDestroy(){this._attachSubscription.unsubscribe(),this._detachSubscription.unsubscribe(),this._backdropSubscription.unsubscribe(),this._positionSubscription.unsubscribe(),this._overlayRef?.dispose()}ngOnChanges(e){this._position&&(this._updatePositionStrategy(this._position),this._overlayRef?.updateSize({width:this._getWidth(),minWidth:this.minWidth,height:this.height,minHeight:this.minHeight}),e.origin&&this.open&&this._position.apply()),e.open&&(this.open?this.attachOverlay():this.detachOverlay())}_createOverlay(){(!this.positions||!this.positions.length)&&(this.positions=oe$1);let e=this._overlayRef=gt$1(this._injector,this._buildConfig());this._attachSubscription=e.attachments().subscribe(()=>this.attach.emit()),this._detachSubscription=e.detachments().subscribe(()=>this.detach.emit()),e.keydownEvents().subscribe(i=>{this.overlayKeydown.next(i),i.keyCode===27&&!this.disableClose&&!Ve$1(i)&&(i.preventDefault(),this.detachOverlay())}),this._overlayRef.outsidePointerEvents().subscribe(i=>{let s=this._getOriginElement(),n=y$1(i);(!s||s!==n&&!s.contains(n))&&this.overlayOutsideClick.next(i)})}_buildConfig(){let e=this._position=this.positionStrategy||this._createPositionStrategy(),i=new B({direction:this._dir||`ltr`,positionStrategy:e,scrollStrategy:this.scrollStrategy,hasBackdrop:this.hasBackdrop,disposeOnNavigation:this.disposeOnNavigation,usePopover:!!this.usePopover});return(this.height||this.height===0)&&(i.height=this.height),(this.minWidth||this.minWidth===0)&&(i.minWidth=this.minWidth),(this.minHeight||this.minHeight===0)&&(i.minHeight=this.minHeight),this.backdropClass&&(i.backdropClass=this.backdropClass),this.panelClass&&(i.panelClass=this.panelClass),i}_updatePositionStrategy(e){let i=this.positions.map(s=>({originX:s.originX,originY:s.originY,overlayX:s.overlayX,overlayY:s.overlayY,offsetX:s.offsetX||this.offsetX,offsetY:s.offsetY||this.offsetY,panelClass:s.panelClass||void 0}));return e.setOrigin(this._getOrigin()).withPositions(i).withFlexibleDimensions(this.flexibleDimensions).withPush(this.push).withGrowAfterOpen(this.growAfterOpen).withViewportMargin(this.viewportMargin).withLockedPosition(this.lockPosition).withTransformOriginOn(this.transformOriginSelector).withPopoverLocation(this.usePopover===null?`global`:this.usePopover)}_createPositionStrategy(){let e=ut$2(this._injector,this._getOrigin());return this._updatePositionStrategy(e),e}_getOrigin(){return this.origin instanceof dt$1?this.origin.elementRef:this.origin}_getOriginElement(){return this.origin instanceof dt$1?this.origin.elementRef.nativeElement:this.origin instanceof Dr?this.origin.nativeElement:typeof Element<`u`&&this.origin instanceof Element?this.origin:null}_getWidth(){return this.width?this.width:this.matchWidth?this._getOriginElement()?.getBoundingClientRect?.().width:void 0}attachOverlay(){this._overlayRef||this._createOverlay();let e=this._overlayRef;e.getConfig().hasBackdrop=this.hasBackdrop,e.updateSize({width:this._getWidth()}),e.hasAttached()||e.attach(this._templatePortal),this.hasBackdrop?this._backdropSubscription=e.backdropClick().subscribe(i=>this.backdropClick.emit(i)):this._backdropSubscription.unsubscribe(),this._positionSubscription.unsubscribe(),this.positionChange.observers.length>0&&(this._positionSubscription=this._position.positionChanges.pipe(kg(()=>this.positionChange.observers.length>0)).subscribe(i=>{this._ngZone.run(()=>this.positionChange.emit(i)),this.positionChange.observers.length===0&&this._positionSubscription.unsubscribe()})),this.open=!0}detachOverlay(){this._overlayRef?.detach(),this._backdropSubscription.unsubscribe(),this._positionSubscription.unsubscribe(),this.open=!1}_assignConfig(e){this.origin=e.origin??this.origin,this.positions=e.positions??this.positions,this.positionStrategy=e.positionStrategy??this.positionStrategy,this.offsetX=e.offsetX??this.offsetX,this.offsetY=e.offsetY??this.offsetY,this.width=e.width??this.width,this.height=e.height??this.height,this.minWidth=e.minWidth??this.minWidth,this.minHeight=e.minHeight??this.minHeight,this.backdropClass=e.backdropClass??this.backdropClass,this.panelClass=e.panelClass??this.panelClass,this.viewportMargin=e.viewportMargin??this.viewportMargin,this.scrollStrategy=e.scrollStrategy??this.scrollStrategy,this.disableClose=e.disableClose??this.disableClose,this.transformOriginSelector=e.transformOriginSelector??this.transformOriginSelector,this.hasBackdrop=e.hasBackdrop??this.hasBackdrop,this.lockPosition=e.lockPosition??this.lockPosition,this.flexibleDimensions=e.flexibleDimensions??this.flexibleDimensions,this.growAfterOpen=e.growAfterOpen??this.growAfterOpen,this.push=e.push??this.push,this.disposeOnNavigation=e.disposeOnNavigation??this.disposeOnNavigation,this.usePopover=e.usePopover??this.usePopover,this.matchWidth=e.matchWidth??this.matchWidth}static ɵfac=function(i){return new(i||o)};static ɵdir=ME({type:o,selectors:[[``,`cdk-connected-overlay`,``],[``,`connected-overlay`,``],[``,`cdkConnectedOverlay`,``]],inputs:{origin:[0,`cdkConnectedOverlayOrigin`,`origin`],positions:[0,`cdkConnectedOverlayPositions`,`positions`],positionStrategy:[0,`cdkConnectedOverlayPositionStrategy`,`positionStrategy`],offsetX:[0,`cdkConnectedOverlayOffsetX`,`offsetX`],offsetY:[0,`cdkConnectedOverlayOffsetY`,`offsetY`],width:[0,`cdkConnectedOverlayWidth`,`width`],height:[0,`cdkConnectedOverlayHeight`,`height`],minWidth:[0,`cdkConnectedOverlayMinWidth`,`minWidth`],minHeight:[0,`cdkConnectedOverlayMinHeight`,`minHeight`],backdropClass:[0,`cdkConnectedOverlayBackdropClass`,`backdropClass`],panelClass:[0,`cdkConnectedOverlayPanelClass`,`panelClass`],viewportMargin:[0,`cdkConnectedOverlayViewportMargin`,`viewportMargin`],scrollStrategy:[0,`cdkConnectedOverlayScrollStrategy`,`scrollStrategy`],open:[0,`cdkConnectedOverlayOpen`,`open`],disableClose:[0,`cdkConnectedOverlayDisableClose`,`disableClose`],transformOriginSelector:[0,`cdkConnectedOverlayTransformOriginOn`,`transformOriginSelector`],hasBackdrop:[2,`cdkConnectedOverlayHasBackdrop`,`hasBackdrop`,n1],lockPosition:[2,`cdkConnectedOverlayLockPosition`,`lockPosition`,n1],flexibleDimensions:[2,`cdkConnectedOverlayFlexibleDimensions`,`flexibleDimensions`,n1],growAfterOpen:[2,`cdkConnectedOverlayGrowAfterOpen`,`growAfterOpen`,n1],push:[2,`cdkConnectedOverlayPush`,`push`,n1],disposeOnNavigation:[2,`cdkConnectedOverlayDisposeOnNavigation`,`disposeOnNavigation`,n1],usePopover:[0,`cdkConnectedOverlayUsePopover`,`usePopover`],matchWidth:[2,`cdkConnectedOverlayMatchWidth`,`matchWidth`,n1],_config:[0,`cdkConnectedOverlay`,`_config`]},outputs:{backdropClick:`backdropClick`,positionChange:`positionChange`,attach:`attach`,detach:`detach`,overlayKeydown:`overlayKeydown`,overlayOutsideClick:`overlayOutsideClick`},exportAs:[`cdkConnectedOverlay`],features:[Hm]})}return o})();var re$1=(()=>{class o{static ɵfac=function(i){return new(i||o)};static ɵmod=TE({type:o});static ɵinj=tu({providers:[Jt],imports:[St$1,Vt,Xe,Xe]})}return o})();var W=class{_box;_destroyed=new Q$1;_resizeSubject=new Q$1;_resizeObserver;_elementObservables=new Map;constructor(r){this._box=r,typeof ResizeObserver<`u`&&(this._resizeObserver=new ResizeObserver(e=>this._resizeSubject.next(e)))}observe(r){return this._elementObservables.has(r)||this._elementObservables.set(r,new _(e=>{let i=this._resizeSubject.subscribe(e);return this._resizeObserver?.observe(r,{box:this._box}),()=>{this._resizeObserver?.unobserve(r),i.unsubscribe(),this._elementObservables.delete(r)}}).pipe(Yt$1(e=>e.some(i=>i.target===r)),Pl({bufferSize:1,refCount:!0}),Rg(this._destroyed))),this._elementObservables.get(r)}destroy(){this._destroyed.next(),this._destroyed.complete(),this._resizeSubject.complete(),this._elementObservables.clear()}};var Ee=(()=>{class t{_cleanupErrorListener;_observers=new Map;_ngZone=T(Ae$2);constructor(){}ngOnDestroy(){for(let[,e]of this._observers)e.destroy();this._observers.clear(),this._cleanupErrorListener?.()}observe(e,i){let n=i?.box||`content-box`;return this._observers.has(n)||this._observers.set(n,new W(n)),this._observers.get(n).observe(e)}static ɵfac=function(i){return new(i||t)};static ɵprov=Er({token:t,factory:t.ɵfac})}return t})();var Ke=[`notch`];var Je=[`*`];var ze=[`iconPrefixContainer`];var Le=[`textPrefixContainer`];var Re=[`iconSuffixContainer`];var De=[`textSuffixContainer`];var et=[`textField`];var tt=[`*`,[[`mat-label`]],[[``,`matPrefix`,``],[``,`matIconPrefix`,``]],[[``,`matTextPrefix`,``]],[[``,`matTextSuffix`,``]],[[``,`matSuffix`,``],[``,`matIconSuffix`,``]],[[`mat-error`],[``,`matError`,``]],[[`mat-hint`,3,`align`,`end`]],[[`mat-hint`,`align`,`end`]]];var it$1=[`*`,`mat-label`,`[matPrefix], [matIconPrefix]`,`[matTextPrefix]`,`[matTextSuffix]`,`[matSuffix], [matIconSuffix]`,`mat-error, [matError]`,`mat-hint:not([align='end'])`,`mat-hint[align='end']`];function nt(t,r){t&1&&Up(0,`span`,21)}function ot$1(t,r){if(t&1&&(wi$1(0,`label`,20),pD(1,1),zE(2,nt,1,0,`span`,21),Uc()),t&2){let e=uD(2);$p(`floating`,e._shouldLabelFloat())(`monitorResize`,e._hasOutline())(`id`,e._labelId),Bp(`for`,e._control.disableAutomaticLabeling?null:e._control.id),Gv(2),QE(!e.hideRequiredMarker&&e._control.required?2:-1)}}function rt(t,r){if(t&1&&zE(0,ot$1,3,5,`label`,20),t&2)QE(uD()._hasFloatingLabel()?0:-1)}function lt(t,r){t&1&&Up(0,`div`,7)}function at(t,r){}function dt(t,r){if(t&1&&Pp(0,at,0,0,`ng-template`,13),t&2){uD(2);$p(`ngTemplateOutlet`,vD(1))}}function mt(t,r){if(t&1&&(wi$1(0,`div`,9),zE(1,dt,1,1,null,13),Uc()),t&2){let e=uD();$p(`matFormFieldNotchedOutlineOpen`,e._shouldLabelFloat()),Gv(),QE(e._forceDisplayInfixLabel()?-1:1)}}function ct(t,r){t&1&&(wi$1(0,`div`,10,2),pD(2,2),Uc())}function st(t,r){t&1&&(wi$1(0,`div`,11,3),pD(2,3),Uc())}function ft$1(t,r){}function ut$1(t,r){if(t&1&&Pp(0,ft$1,0,0,`ng-template`,13),t&2){uD();$p(`ngTemplateOutlet`,vD(1))}}function pt$1(t,r){t&1&&(wi$1(0,`div`,14,4),pD(2,4),Uc())}function ht(t,r){t&1&&(wi$1(0,`div`,15,5),pD(2,5),Uc())}function bt(t,r){t&1&&Up(0,`div`,16)}function xt(t,r){t&1&&(wi$1(0,`div`,18),pD(1,6),Uc())}function _t(t,r){if(t&1&&(wi$1(0,`mat-hint`,22),HD(1),Uc()),t&2){let e=uD(2);$p(`id`,e._hintLabelId),Gv(),dh(e.hintLabel)}}function gt(t,r){if(t&1&&(wi$1(0,`div`,19),zE(1,_t,2,2,`mat-hint`,22),pD(2,7),Up(3,`div`,23),pD(4,8),Uc()),t&2){let e=uD();Gv(),QE(e.hintLabel?1:-1)}}var G=(()=>{class t{static ɵfac=function(i){return new(i||t)};static ɵdir=ME({type:t,selectors:[[`mat-label`]]})}return t})();var Z=new A$2(`MatError`);var vt=(()=>{class t{id=T(Bt$1).getId(`mat-mdc-error-`);static ɵfac=function(i){return new(i||t)};static ɵdir=ME({type:t,selectors:[[`mat-error`],[``,`matError`,``]],hostAttrs:[1,`mat-mdc-form-field-error`,`mat-mdc-form-field-bottom-align`],hostVars:1,hostBindings:function(i,n){i&2&&zp(`id`,n.id)},inputs:{id:`id`},features:[YD([{provide:Z,useExisting:t}])]})}return t})();var U=(()=>{class t{align=`start`;id=T(Bt$1).getId(`mat-mdc-hint-`);static ɵfac=function(i){return new(i||t)};static ɵdir=ME({type:t,selectors:[[`mat-hint`]],hostAttrs:[1,`mat-mdc-form-field-hint`,`mat-mdc-form-field-bottom-align`],hostVars:4,hostBindings:function(i,n){i&2&&(zp(`id`,n.id),Bp(`align`,null),ih(`mat-mdc-form-field-hint-end`,n.align===`end`))},inputs:{align:`align`,id:`id`}})}return t})();var Be=new A$2(`MatPrefix`);var $$1=new A$2(`MatSuffix`);var St=(()=>{class t{set _isTextSelector(e){this._isText=!0}_isText=!1;static ɵfac=function(i){return new(i||t)};static ɵdir=ME({type:t,selectors:[[``,`matSuffix`,``],[``,`matIconSuffix`,``],[``,`matTextSuffix`,``]],inputs:{_isTextSelector:[0,`matTextSuffix`,`_isTextSelector`]},features:[YD([{provide:$$1,useExisting:t}])]})}return t})();var qe=new A$2(`FloatingLabelParent`);var Te=(()=>{class t{_elementRef=T(Dr);get floating(){return this._floating}set floating(e){this._floating=e,this.monitorResize&&this._handleResize()}_floating=!1;get monitorResize(){return this._monitorResize}set monitorResize(e){this._monitorResize=e,this._monitorResize?this._subscribeToResize():this._resizeSubscription.unsubscribe()}_monitorResize=!1;_resizeObserver=T(Ee);_ngZone=T(Ae$2);_parent=T(qe);_resizeSubscription=new j$1;ngOnDestroy(){this._resizeSubscription.unsubscribe()}getWidth(){return yt(this._elementRef.nativeElement)}get element(){return this._elementRef.nativeElement}_handleResize(){setTimeout(()=>this._parent._handleLabelResized())}_subscribeToResize(){this._resizeSubscription.unsubscribe(),this._ngZone.runOutsideAngular(()=>{this._resizeSubscription=this._resizeObserver.observe(this._elementRef.nativeElement,{box:`border-box`}).subscribe(()=>this._handleResize())})}static ɵfac=function(i){return new(i||t)};static ɵdir=ME({type:t,selectors:[[`label`,`matFormFieldFloatingLabel`,``]],hostAttrs:[1,`mdc-floating-label`,`mat-mdc-floating-label`],hostVars:2,hostBindings:function(i,n){i&2&&ih(`mdc-floating-label--float-above`,n.floating)},inputs:{floating:`floating`,monitorResize:`monitorResize`}})}return t})();function yt(t){let r=t;if(r.offsetParent!==null)return r.scrollWidth;let e=r.cloneNode(!0);e.style.setProperty(`position`,`absolute`),e.style.setProperty(`transform`,`translate(-9999px, -9999px)`),document.documentElement.appendChild(e);let i=e.scrollWidth;return e.remove(),i}var Oe=`mdc-line-ripple--active`;var O=`mdc-line-ripple--deactivating`;var ke=(()=>{class t{_elementRef=T(Dr);_cleanupTransitionEnd;constructor(){let e=T(Ae$2),i=T(Wa);e.runOutsideAngular(()=>{this._cleanupTransitionEnd=i.listen(this._elementRef.nativeElement,`transitionend`,this._handleTransitionEnd)})}activate(){let e=this._elementRef.nativeElement.classList;e.remove(O),e.add(Oe)}deactivate(){this._elementRef.nativeElement.classList.add(O)}_handleTransitionEnd=e=>{let i=this._elementRef.nativeElement.classList,n=i.contains(O);e.propertyName===`opacity`&&n&&i.remove(Oe,O)};ngOnDestroy(){this._cleanupTransitionEnd()}static ɵfac=function(i){return new(i||t)};static ɵdir=ME({type:t,selectors:[[`div`,`matFormFieldLineRipple`,``]],hostAttrs:[1,`mdc-line-ripple`]})}return t})();var Pe=(()=>{class t{_elementRef=T(Dr);_ngZone=T(Ae$2);open=!1;_notch;ngAfterViewInit(){let e=this._elementRef.nativeElement,i=e.querySelector(`.mdc-floating-label`);i?(e.classList.add(`mdc-notched-outline--upgraded`),typeof requestAnimationFrame==`function`&&(i.style.transitionDuration=`0s`,this._ngZone.runOutsideAngular(()=>{requestAnimationFrame(()=>i.style.transitionDuration=``)}))):e.classList.add(`mdc-notched-outline--no-label`)}_setNotchWidth(e){let i=this._notch.nativeElement;!this.open||!e?i.style.width=``:i.style.width=`calc(${e}px * var(--mat-mdc-form-field-floating-label-scale, 0.75) + 9px)`}_setMaxWidth(e){this._notch.nativeElement.style.setProperty(`--mat-form-field-notch-max-width`,`calc(100% - ${e}px)`)}static ɵfac=function(i){return new(i||t)};static ɵcmp=wE({type:t,selectors:[[`div`,`matFormFieldNotchedOutline`,``]],viewQuery:function(i,n){if(i&1&&Xp(Ke,5),i&2){let o;gD(o=mD())&&(n._notch=o.first)}},hostAttrs:[1,`mdc-notched-outline`],hostVars:2,hostBindings:function(i,n){i&2&&ih(`mdc-notched-outline--notched`,n.open)},inputs:{open:[0,`matFormFieldNotchedOutlineOpen`,`open`]},ngContentSelectors:Je,decls:5,vars:0,consts:[[`notch`,``],[1,`mat-mdc-notch-piece`,`mdc-notched-outline__leading`],[1,`mat-mdc-notch-piece`,`mdc-notched-outline__notch`],[1,`mat-mdc-notch-piece`,`mdc-notched-outline__trailing`]],template:function(i,n){i&1&&(fD(),qp(0,`div`,1),qc(1,`div`,2,0),pD(3),Wc(),qp(4,`div`,3))},encapsulation:2})}return t})();var He=(()=>{class t{value=null;stateChanges;id;placeholder;ngControl=null;focused=!1;empty=!1;shouldLabelFloat=!1;required=!1;disabled=!1;errorState=!1;controlType;autofilled;userAriaDescribedBy;disableAutomaticLabeling;describedByIds;static ɵfac=function(i){return new(i||t)};static ɵdir=ME({type:t})}return t})();var Qe=new A$2(`MatFormField`);var je=new A$2(`MAT_FORM_FIELD_DEFAULT_OPTIONS`);var Ae=`fill`;var Nt$1=`auto`;var Ie=`fixed`;var Ft$1=`translateY(-50%)`;var We=(()=>{class t{_elementRef=T(Dr);_changeDetectorRef=T(e1);_platform=T(l$1);_idGenerator=T(Bt$1);_ngZone=T(Ae$2);_defaults=T(je,{optional:!0});_currentDirection;_textField;_iconPrefixContainer;_textPrefixContainer;_iconSuffixContainer;_textSuffixContainer;_floatingLabel;_notchedOutline;_lineRipple;_iconPrefixContainerSignal=KF(`iconPrefixContainer`);_textPrefixContainerSignal=KF(`textPrefixContainer`);_iconSuffixContainerSignal=KF(`iconSuffixContainer`);_textSuffixContainerSignal=KF(`textSuffixContainer`);_prefixSuffixContainers=cw(()=>[this._iconPrefixContainerSignal(),this._textPrefixContainerSignal(),this._iconSuffixContainerSignal(),this._textSuffixContainerSignal()].map(e=>e?.nativeElement).filter(e=>e!==void 0));_formFieldControl;_prefixChildren;_suffixChildren;_errorChildren;_hintChildren;_labelChild=JF(G);get hideRequiredMarker(){return this._hideRequiredMarker}set hideRequiredMarker(e){this._hideRequiredMarker=mi$1(e)}_hideRequiredMarker=!1;color=`primary`;get floatLabel(){return this._floatLabel||this._defaults?.floatLabel||Nt$1}set floatLabel(e){e!==this._floatLabel&&(this._floatLabel=e,this._changeDetectorRef.markForCheck())}_floatLabel;get appearance(){return this._appearanceSignal()}set appearance(e){let i=e||this._defaults?.appearance||Ae;this._appearanceSignal.set(i)}_appearanceSignal=Uo(Ae);get subscriptSizing(){return this._subscriptSizing||this._defaults?.subscriptSizing||Ie}set subscriptSizing(e){this._subscriptSizing=e||this._defaults?.subscriptSizing||Ie}_subscriptSizing=null;get hintLabel(){return this._hintLabel}set hintLabel(e){this._hintLabel=e,this._processHints()}_hintLabel=``;_hasIconPrefix=!1;_hasTextPrefix=!1;_hasIconSuffix=!1;_hasTextSuffix=!1;_labelId=this._idGenerator.getId(`mat-mdc-form-field-label-`);_hintLabelId=this._idGenerator.getId(`mat-mdc-hint-`);_describedByIds;get _control(){return this._explicitFormFieldControl||this._formFieldControl}set _control(e){this._explicitFormFieldControl=e}_destroyed=new Q$1;_isFocused=null;_explicitFormFieldControl;_previousControl=null;_previousControlValidatorFn=null;_stateChanges;_valueChanges;_describedByChanges;_outlineLabelOffsetResizeObserver=null;_animationsDisabled=z$1();constructor(){let e=this._defaults,i=T(Yn$1);e&&(e.appearance&&(this.appearance=e.appearance),this._hideRequiredMarker=!!e?.hideRequiredMarker,e.color&&(this.color=e.color)),Ku(()=>this._currentDirection=i.valueSignal()),this._syncOutlineLabelOffset()}ngAfterViewInit(){this._updateFocusState(),this._animationsDisabled||this._ngZone.runOutsideAngular(()=>{setTimeout(()=>{this._elementRef.nativeElement.classList.add(`mat-form-field-animations-enabled`)},300)}),this._changeDetectorRef.detectChanges()}ngAfterContentInit(){this._assertFormFieldControl(),this._initializeSubscript(),this._initializePrefixAndSuffix()}ngAfterContentChecked(){this._assertFormFieldControl(),this._control!==this._previousControl&&(this._initializeControl(this._previousControl),this._control.ngControl&&this._control.ngControl.control&&(this._previousControlValidatorFn=this._control.ngControl.control.validator),this._previousControl=this._control),this._control.ngControl&&this._control.ngControl.control&&this._control.ngControl.control.validator!==this._previousControlValidatorFn&&this._changeDetectorRef.markForCheck()}ngOnDestroy(){this._outlineLabelOffsetResizeObserver?.disconnect(),this._stateChanges?.unsubscribe(),this._valueChanges?.unsubscribe(),this._describedByChanges?.unsubscribe(),this._destroyed.next(),this._destroyed.complete()}getLabelId=cw(()=>this._hasFloatingLabel()?this._labelId:null);getConnectedOverlayOrigin(){return this._textField||this._elementRef}_animateAndLockLabel(){this._hasFloatingLabel()&&(this.floatLabel=`always`)}_initializeControl(e){let i=this._control,n=`mat-mdc-form-field-type-`;e&&this._elementRef.nativeElement.classList.remove(n+e.controlType),i.controlType&&this._elementRef.nativeElement.classList.add(n+i.controlType),this._stateChanges?.unsubscribe(),this._stateChanges=i.stateChanges.subscribe(()=>{this._updateFocusState(),this._changeDetectorRef.markForCheck()}),this._describedByChanges?.unsubscribe(),this._describedByChanges=i.stateChanges.pipe(Ag([void 0,void 0]),ue$1(()=>[i.errorState,i.userAriaDescribedBy]),Mg(),Yt$1(([[o,l],[b,k]])=>o!==b||l!==k)).subscribe(()=>this._syncDescribedByIds()),this._valueChanges?.unsubscribe(),i.ngControl&&i.ngControl.valueChanges&&(this._valueChanges=i.ngControl.valueChanges.pipe(Rg(this._destroyed)).subscribe(()=>this._changeDetectorRef.markForCheck()))}_checkPrefixAndSuffixTypes(){this._hasIconPrefix=!!this._prefixChildren.find(e=>!e._isText),this._hasTextPrefix=!!this._prefixChildren.find(e=>e._isText),this._hasIconSuffix=!!this._suffixChildren.find(e=>!e._isText),this._hasTextSuffix=!!this._suffixChildren.find(e=>e._isText)}_initializePrefixAndSuffix(){this._checkPrefixAndSuffixTypes(),hg(this._prefixChildren.changes,this._suffixChildren.changes).subscribe(()=>{this._checkPrefixAndSuffixTypes(),this._changeDetectorRef.markForCheck()})}_initializeSubscript(){this._hintChildren.changes.subscribe(()=>{this._processHints(),this._changeDetectorRef.markForCheck()}),this._errorChildren.changes.subscribe(()=>{this._syncDescribedByIds(),this._changeDetectorRef.markForCheck()}),this._validateHints(),this._syncDescribedByIds()}_assertFormFieldControl(){this._control}_updateFocusState(){let e=this._control.focused;e&&!this._isFocused?(this._isFocused=!0,this._lineRipple?.activate()):!e&&(this._isFocused||this._isFocused===null)&&(this._isFocused=!1,this._lineRipple?.deactivate()),this._elementRef.nativeElement.classList.toggle(`mat-focused`,e),this._textField?.nativeElement.classList.toggle(`mdc-text-field--focused`,e)}_syncOutlineLabelOffset(){o1({earlyRead:()=>{if(this._appearanceSignal()!==`outline`)return this._outlineLabelOffsetResizeObserver?.disconnect(),null;if(globalThis.ResizeObserver){this._outlineLabelOffsetResizeObserver||=new globalThis.ResizeObserver(()=>{this._writeOutlinedLabelStyles(this._getOutlinedLabelOffset())});for(let e of this._prefixSuffixContainers())this._outlineLabelOffsetResizeObserver.observe(e,{box:`border-box`})}return this._getOutlinedLabelOffset()},write:e=>this._writeOutlinedLabelStyles(e())})}_shouldAlwaysFloat(){return this.floatLabel===`always`}_hasOutline(){return this.appearance===`outline`}_forceDisplayInfixLabel(){return!this._platform.isBrowser&&this._prefixChildren.length&&!this._shouldLabelFloat()}_hasFloatingLabel=cw(()=>!!this._labelChild());_shouldLabelFloat(){return this._hasFloatingLabel()?this._control.shouldLabelFloat||this._shouldAlwaysFloat():!1}_shouldForward(e){let i=this._control?this._control.ngControl:null;return i&&i[e]}_getSubscriptMessageType(){return this._errorChildren&&this._errorChildren.length>0&&this._control.errorState?`error`:`hint`}_handleLabelResized(){this._refreshOutlineNotchWidth()}_refreshOutlineNotchWidth(){!this._hasOutline()||!this._floatingLabel||!this._shouldLabelFloat()?this._notchedOutline?._setNotchWidth(0):this._notchedOutline?._setNotchWidth(this._floatingLabel.getWidth())}_processHints(){this._validateHints(),this._syncDescribedByIds()}_validateHints(){this._hintChildren}_syncDescribedByIds(){if(this._control){let e=[];if(this._control.userAriaDescribedBy&&typeof this._control.userAriaDescribedBy==`string`&&e.push(...this._control.userAriaDescribedBy.split(` `)),this._getSubscriptMessageType()===`hint`){let o=this._hintChildren?this._hintChildren.find(b=>b.align===`start`):null,l=this._hintChildren?this._hintChildren.find(b=>b.align===`end`):null;o?e.push(o.id):this._hintLabel&&e.push(this._hintLabelId),l&&e.push(l.id)}else this._errorChildren&&e.push(...this._errorChildren.map(o=>o.id));let i=this._control.describedByIds,n;if(i){let o=this._describedByIds||e;n=e.concat(i.filter(l=>l&&!o.includes(l)))}else n=e;this._control.setDescribedByIds(n),this._describedByIds=e}}_getOutlinedLabelOffset(){if(!this._hasOutline()||!this._floatingLabel)return null;if(!this._iconPrefixContainer&&!this._textPrefixContainer)return[``,null];if(!this._isAttachedToDom())return null;let e=this._iconPrefixContainer?.nativeElement,i=this._textPrefixContainer?.nativeElement,n=this._iconSuffixContainer?.nativeElement,o=this._textSuffixContainer?.nativeElement,l=e?.getBoundingClientRect().width??0,b=i?.getBoundingClientRect().width??0,k=n?.getBoundingClientRect().width??0,Ge=o?.getBoundingClientRect().width??0;return[`var(--mat-mdc-form-field-label-transform, ${Ft$1} translateX(${`calc(${this._currentDirection===`rtl`?`-1`:`1`} * (${`${l+b}px`} + var(--mat-mdc-form-field-label-offset-x, 0px)))`}))`,l+b+k+Ge]}_writeOutlinedLabelStyles(e){if(e!==null){let[i,n]=e;this._floatingLabel&&(this._floatingLabel.element.style.transform=i),n!==null&&this._notchedOutline?._setMaxWidth(n)}}_isAttachedToDom(){let e=this._elementRef.nativeElement;if(e.getRootNode){let i=e.getRootNode();return i&&i!==e}return document.documentElement.contains(e)}static ɵfac=function(i){return new(i||t)};static ɵcmp=wE({type:t,selectors:[[`mat-form-field`]],contentQueries:function(i,n,o){if(i&1&&(eh(o,n._labelChild,G,5),Jp(o,He,5)(o,Be,5)(o,$$1,5)(o,Z,5)(o,U,5)),i&2){yD();let l;gD(l=mD())&&(n._formFieldControl=l.first),gD(l=mD())&&(n._prefixChildren=l),gD(l=mD())&&(n._suffixChildren=l),gD(l=mD())&&(n._errorChildren=l),gD(l=mD())&&(n._hintChildren=l)}},viewQuery:function(i,n){if(i&1&&(th(n._iconPrefixContainerSignal,ze,5)(n._textPrefixContainerSignal,Le,5)(n._iconSuffixContainerSignal,Re,5)(n._textSuffixContainerSignal,De,5),Xp(et,5)(ze,5)(Le,5)(Re,5)(De,5)(Te,5)(Pe,5)(ke,5)),i&2){yD(4);let o;gD(o=mD())&&(n._textField=o.first),gD(o=mD())&&(n._iconPrefixContainer=o.first),gD(o=mD())&&(n._textPrefixContainer=o.first),gD(o=mD())&&(n._iconSuffixContainer=o.first),gD(o=mD())&&(n._textSuffixContainer=o.first),gD(o=mD())&&(n._floatingLabel=o.first),gD(o=mD())&&(n._notchedOutline=o.first),gD(o=mD())&&(n._lineRipple=o.first)}},hostAttrs:[1,`mat-mdc-form-field`],hostVars:38,hostBindings:function(i,n){i&2&&ih(`mat-mdc-form-field-label-always-float`,n._shouldAlwaysFloat())(`mat-mdc-form-field-has-icon-prefix`,n._hasIconPrefix)(`mat-mdc-form-field-has-icon-suffix`,n._hasIconSuffix)(`mat-form-field-invalid`,n._control.errorState)(`mat-form-field-disabled`,n._control.disabled)(`mat-form-field-autofilled`,n._control.autofilled)(`mat-form-field-appearance-fill`,n.appearance==`fill`)(`mat-form-field-appearance-outline`,n.appearance==`outline`)(`mat-form-field-hide-placeholder`,n._hasFloatingLabel()&&!n._shouldLabelFloat())(`mat-primary`,n.color!==`accent`&&n.color!==`warn`)(`mat-accent`,n.color===`accent`)(`mat-warn`,n.color===`warn`)(`ng-untouched`,n._shouldForward(`untouched`))(`ng-touched`,n._shouldForward(`touched`))(`ng-pristine`,n._shouldForward(`pristine`))(`ng-dirty`,n._shouldForward(`dirty`))(`ng-valid`,n._shouldForward(`valid`))(`ng-invalid`,n._shouldForward(`invalid`))(`ng-pending`,n._shouldForward(`pending`))},inputs:{hideRequiredMarker:`hideRequiredMarker`,color:`color`,floatLabel:`floatLabel`,appearance:`appearance`,subscriptSizing:`subscriptSizing`,hintLabel:`hintLabel`},exportAs:[`matFormField`],features:[YD([{provide:Qe,useExisting:t},{provide:qe,useExisting:t}])],ngContentSelectors:it$1,decls:18,vars:21,consts:[[`labelTemplate`,``],[`textField`,``],[`iconPrefixContainer`,``],[`textPrefixContainer`,``],[`textSuffixContainer`,``],[`iconSuffixContainer`,``],[1,`mat-mdc-text-field-wrapper`,`mdc-text-field`,3,`click`],[1,`mat-mdc-form-field-focus-overlay`],[1,`mat-mdc-form-field-flex`],[`matFormFieldNotchedOutline`,``,3,`matFormFieldNotchedOutlineOpen`],[1,`mat-mdc-form-field-icon-prefix`],[1,`mat-mdc-form-field-text-prefix`],[1,`mat-mdc-form-field-infix`],[3,`ngTemplateOutlet`],[1,`mat-mdc-form-field-text-suffix`],[1,`mat-mdc-form-field-icon-suffix`],[`matFormFieldLineRipple`,``],[`aria-atomic`,`true`,`aria-live`,`polite`,1,`mat-mdc-form-field-subscript-wrapper`,`mat-mdc-form-field-bottom-align`],[1,`mat-mdc-form-field-error-wrapper`],[1,`mat-mdc-form-field-hint-wrapper`],[`matFormFieldFloatingLabel`,``,3,`floating`,`monitorResize`,`id`],[`aria-hidden`,`true`,1,`mat-mdc-form-field-required-marker`,`mdc-floating-label--required`],[3,`id`],[1,`mat-mdc-form-field-hint-spacer`]],template:function(i,n){if(i&1&&(fD(tt),Pp(0,rt,1,1,`ng-template`,null,0,iw),wi$1(2,`div`,6,1),Zp(`click`,function(l){return n._control.onContainerClick(l)}),zE(4,lt,1,0,`div`,7),wi$1(5,`div`,8),zE(6,mt,2,2,`div`,9),zE(7,ct,3,0,`div`,10),zE(8,st,3,0,`div`,11),wi$1(9,`div`,12),zE(10,ut$1,1,1,null,13),pD(11),Uc(),zE(12,pt$1,3,0,`div`,14),zE(13,ht,3,0,`div`,15),Uc(),zE(14,bt,1,0,`div`,16),Uc(),wi$1(15,`div`,17),zE(16,xt,2,0,`div`,18)(17,gt,5,1,`div`,19),Uc()),i&2){let o;Gv(2),ih(`mdc-text-field--filled`,!n._hasOutline())(`mdc-text-field--outlined`,n._hasOutline())(`mdc-text-field--no-label`,!n._hasFloatingLabel())(`mdc-text-field--disabled`,n._control.disabled)(`mdc-text-field--invalid`,n._control.errorState),Gv(2),QE(!n._hasOutline()&&!n._control.disabled?4:-1),Gv(2),QE(n._hasOutline()?6:-1),Gv(),QE(n._hasIconPrefix?7:-1),Gv(),QE(n._hasTextPrefix?8:-1),Gv(2),QE(!n._hasOutline()||n._forceDisplayInfixLabel()?10:-1),Gv(2),QE(n._hasTextSuffix?12:-1),Gv(),QE(n._hasIconSuffix?13:-1),Gv(),QE(n._hasOutline()?-1:14),Gv(),ih(`mat-mdc-form-field-subscript-dynamic-size`,n.subscriptSizing===`dynamic`);let l=n._getSubscriptMessageType();Gv(),QE((o=l)===`error`?16:o===`hint`?17:-1)}},dependencies:[Te,Pe,Da,ke,U],styles:[`.mdc-text-field {
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
`],encapsulation:2})}return t})();var ni$1=(()=>{class t{static ɵfac=function(i){return new(i||t)};static ɵmod=TE({type:t});static ɵinj=tu({imports:[Fe$2,We,St$1]})}return t})();var li$1=(()=>{class t{isErrorState(e,i){return!!(e&&e.invalid&&(e.touched||i&&i.submitted))}isSignalErrorState(e){if(!e)return!1;let i=e().invalid(),n=e().touched();return i&&n}static ɵfac=function(i){return new(i||t)};static ɵprov=Er({token:t,factory:t.ɵfac})}return t})();var Ve=class{_defaultMatcher;_parentFormGroup;_parentForm;_stateChanges;errorState=!1;matcher;ngControl;formField;constructor(r,e,i,n,o){this._defaultMatcher=r,this._parentFormGroup=i,this._parentForm=n,this._stateChanges=o,e?Go(e.field)&&!e.updateValueAndValidity?(this.formField=e,this.ngControl=null):(this.formField=null,this.ngControl=e):this.ngControl=this.formField=null}updateErrorState(){let r=this.errorState,e=this._getCurrentErrorState(this.matcher||this._defaultMatcher);e!==r&&(this.errorState=e,this._stateChanges.next())}_getCurrentErrorState(r){if(this.formField&&r?.isSignalErrorState)return r.isSignalErrorState(this.formField.field())??!1;let e=this._parentFormGroup||this._parentForm,i=this.ngControl?this.ngControl.control:null;return r?.isErrorState(i,e)??!1}};var ki=`@`;var Mi=(()=>{class n{doc;delegate;zone;animationType;moduleImpl;_rendererFactoryPromise=null;scheduler=null;injector=T(ye$1);loadingSchedulerFn=T(wi,{optional:!0});_engine;constructor(t,e,i,r,a){this.doc=t,this.delegate=e,this.zone=i,this.animationType=r,this.moduleImpl=a}ngOnDestroy(){this._engine?.flush()}loadImpl(){let t=()=>this.moduleImpl??import(`./chunk-BlwrasMw.js`).then(i=>i),e;return this.loadingSchedulerFn?e=this.loadingSchedulerFn(t):e=t(),e.catch(i=>{throw new M$1(5300,!1)}).then(({ɵcreateEngine:i,ɵAnimationRendererFactory:r})=>{this._engine=i(this.animationType,this.doc);let a=new r(this.delegate,this._engine,this.zone);return this.delegate=a,a})}createRenderer(t,e){let i=this.delegate.createRenderer(t,e);if(i.ɵtype===0)return i;typeof i.throwOnSyntheticProps==`boolean`&&(i.throwOnSyntheticProps=!1);let r=new ne(i);return e?.data?.animation&&!this._rendererFactoryPromise&&(this._rendererFactoryPromise=this.loadImpl()),this._rendererFactoryPromise?.then(a=>{let d=a.createRenderer(t,e);r.use(d),this.scheduler??=this.injector.get(Re$1,null,{optional:!0}),this.scheduler?.notify(10)}).catch(a=>{r.use(i)}),r}begin(){this.delegate.begin?.()}end(){this.delegate.end?.()}whenRenderingDone(){return this.delegate.whenRenderingDone?.()??Promise.resolve()}componentReplaced(t){this._engine?.flush(),this.delegate.componentReplaced?.(t)}static ɵfac=function(e){HI()};static ɵprov=ae$2({token:n,factory:n.ɵfac})}return n})();var ne=class{delegate;replay=[];ɵtype=1;constructor(o){this.delegate=o}use(o){if(this.delegate=o,this.replay!==null){for(let t of this.replay)t(o);this.replay=null}}get data(){return this.delegate.data}destroy(){this.replay=null,this.delegate.destroy()}createElement(o,t){return this.delegate.createElement(o,t)}createComment(o){return this.delegate.createComment(o)}createText(o){return this.delegate.createText(o)}get destroyNode(){return this.delegate.destroyNode}appendChild(o,t){this.delegate.appendChild(o,t)}insertBefore(o,t,e,i){this.delegate.insertBefore(o,t,e,i)}removeChild(o,t,e,i){this.delegate.removeChild(o,t,e,i)}selectRootElement(o,t){return this.delegate.selectRootElement(o,t)}parentNode(o){return this.delegate.parentNode(o)}nextSibling(o){return this.delegate.nextSibling(o)}setAttribute(o,t,e,i){this.delegate.setAttribute(o,t,e,i)}removeAttribute(o,t,e){this.delegate.removeAttribute(o,t,e)}addClass(o,t){this.delegate.addClass(o,t)}removeClass(o,t){this.delegate.removeClass(o,t)}setStyle(o,t,e,i){this.delegate.setStyle(o,t,e,i)}removeStyle(o,t,e){this.delegate.removeStyle(o,t,e)}setProperty(o,t,e){this.shouldReplay(t)&&this.replay.push(i=>i.setProperty(o,t,e)),this.delegate.setProperty(o,t,e)}setValue(o,t){this.delegate.setValue(o,t)}listen(o,t,e,i){return this.shouldReplay(t)&&this.replay.push(r=>r.listen(o,t,e,i)),this.delegate.listen(o,t,e,i)}shouldReplay(o){return this.replay!==null&&o.startsWith(ki)}};var wi=new A$2(``);function Hn(n=`animations`){return mt$1(`NgAsyncAnimations`),Mo([{provide:mr,useFactory:()=>new Mi(T(ir),T(xr),T(Ae$2),n)},{provide:Cm,useValue:n===`noop`?`NoopAnimations`:`BrowserAnimations`}])}var Ti=/^\d{4}-\d{2}-\d{2}(?:T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|(?:(?:\+|-)\d{2}:\d{2}))?)?$/;var Ii=/^(\d?\d)[:.](\d?\d)(?:[:.](\d?\d))?\s*(AM|PM)?$/i;function ie(n,o){let t=Array(n);for(let e=0;e<n;e++)t[e]=o(e);return t}var Ci=(()=>{class n extends l{_matDateLocale=T(m,{optional:!0});constructor(){super();let t=T(m,{optional:!0});t!==void 0&&(this._matDateLocale=t),super.setLocale(this._matDateLocale)}getYear(t){return t.getFullYear()}getMonth(t){return t.getMonth()}getDate(t){return t.getDate()}getDayOfWeek(t){return t.getDay()}getMonthNames(t){let e=new Intl.DateTimeFormat(this.locale,{month:t,timeZone:`utc`});return ie(12,i=>this._format(e,new Date(2017,i,1)))}getDateNames(){let t=new Intl.DateTimeFormat(this.locale,{day:`numeric`,timeZone:`utc`});return ie(31,e=>this._format(t,new Date(2017,0,e+1)))}getDayOfWeekNames(t){let e=new Intl.DateTimeFormat(this.locale,{weekday:t,timeZone:`utc`});return ie(7,i=>this._format(e,new Date(2017,0,i+1)))}getYearName(t){let e=new Intl.DateTimeFormat(this.locale,{year:`numeric`,timeZone:`utc`});return this._format(e,t)}getFirstDayOfWeek(){if(typeof Intl<`u`&&Intl.Locale){let t=new Intl.Locale(this.locale),e=(t.getWeekInfo?.()||t.weekInfo)?.firstDay??0;return e===7?0:e}return 0}getNumDaysInMonth(t){return this.getDate(this._createDateWithOverflow(this.getYear(t),this.getMonth(t)+1,0))}clone(t){return new Date(t.getTime())}createDate(t,e,i){let r=this._createDateWithOverflow(t,e,i);return r.getMonth(),r}today(){return new Date}parse(t,e){return typeof t==`number`?new Date(t):t?new Date(Date.parse(t)):null}format(t,e){if(!this.isValid(t))throw Error(`NativeDateAdapter: Cannot format invalid date.`);let i=new Intl.DateTimeFormat(this.locale,s(r({},e),{timeZone:`utc`}));return this._format(i,t)}addCalendarYears(t,e){return this.addCalendarMonths(t,e*12)}addCalendarMonths(t,e){let i=this._createDateWithOverflow(this.getYear(t),this.getMonth(t)+e,this.getDate(t));return this.getMonth(i)!=((this.getMonth(t)+e)%12+12)%12&&(i=this._createDateWithOverflow(this.getYear(i),this.getMonth(i),0)),i}addCalendarDays(t,e){return this._createDateWithOverflow(this.getYear(t),this.getMonth(t),this.getDate(t)+e)}toIso8601(t){return[t.getUTCFullYear(),this._2digit(t.getUTCMonth()+1),this._2digit(t.getUTCDate())].join(`-`)}deserialize(t){if(typeof t==`string`){if(!t)return null;if(Ti.test(t)){let e=new Date(t);if(this.isValid(e))return e}}return super.deserialize(t)}isDateInstance(t){return t instanceof Date}isValid(t){return!isNaN(t.getTime())}invalid(){return new Date(NaN)}setTime(t,e,i,r){let a=this.clone(t);return a.setHours(e,i,r,0),a}getHours(t){return t.getHours()}getMinutes(t){return t.getMinutes()}getSeconds(t){return t.getSeconds()}parseTime(t,e){if(typeof t!=`string`)return t instanceof Date?new Date(t.getTime()):null;let i=t.trim();if(i.length===0)return null;let r=this._parseTimeString(i);if(r===null){let a=i.replace(/[^0-9:(AM|PM)]/gi,``).trim();a.length>0&&(r=this._parseTimeString(a))}return r||this.invalid()}addSeconds(t,e){return new Date(t.getTime()+e*1e3)}_createDateWithOverflow(t,e,i){let r=new Date;return r.setFullYear(t,e,i),r.setHours(0,0,0,0),r}_2digit(t){return(`00`+t).slice(-2)}_format(t,e){let i=new Date;return i.setUTCFullYear(e.getFullYear(),e.getMonth(),e.getDate()),i.setUTCHours(e.getHours(),e.getMinutes(),e.getSeconds(),e.getMilliseconds()),t.format(i)}_parseTimeString(t){let e=t.toUpperCase().match(Ii);if(e){let i=parseInt(e[1]),r=parseInt(e[2]),a=e[3]==null?void 0:parseInt(e[3]),d=e[4];if(i===12?i=d===`AM`?0:i:d===`PM`&&(i+=12),oe(i,0,23)&&oe(r,0,59)&&(a==null||oe(a,0,59)))return this.setTime(this.today(),i,r,a||0)}return null}static ɵfac=function(e){return new(e||n)};static ɵprov=Er({token:n,factory:n.ɵfac,autoProvided:!1})}return n})();function oe(n,o,t){return!isNaN(n)&&n>=o&&n<=t}var Ei={parse:{dateInput:null,timeInput:null},display:{dateInput:{year:`numeric`,month:`numeric`,day:`numeric`},timeInput:{hour:`numeric`,minute:`numeric`},monthYearLabel:{year:`numeric`,month:`short`},dateA11yLabel:{year:`numeric`,month:`long`,day:`numeric`},monthYearA11yLabel:{year:`numeric`,month:`long`},timeOptionLabel:{hour:`numeric`,minute:`numeric`}}};function $n(n=Ei){return[{provide:l,useClass:Ci},{provide:d,useValue:n}]}var pt=`PERFORM_ACTION`;var Di=`REFRESH`;var Xn=`RESET`;var Kn=`ROLLBACK`;var Qn=`COMMIT`;var Jn=`SWEEP`;var ti=`TOGGLE_ACTION`;var Oi=`SET_ACTIONS_ACTIVE`;var ei=`JUMP_TO_STATE`;var ni=`JUMP_TO_ACTION`;var be=`IMPORT_STATE`;var ii=`LOCK_CHANGES`;var oi=`PAUSE_RECORDING`;var it=class{constructor(o,t){if(this.action=o,this.timestamp=t,this.type=pt,typeof o.type>`u`)throw new Error(`Actions may not have an undefined "type" property. Have you misspelled a constant?`)}};var re=class{constructor(){this.type=Di}};var ae=class{constructor(o){this.timestamp=o,this.type=Xn}};var se=class{constructor(o){this.timestamp=o,this.type=Kn}};var ce=class{constructor(o){this.timestamp=o,this.type=Qn}};var le=class{constructor(){this.type=Jn}};var de=class{constructor(o){this.id=o,this.type=ti}};var me=class{constructor(o){this.index=o,this.type=ei}};var ue=class{constructor(o){this.actionId=o,this.type=ni}};var pe=class{constructor(o){this.nextLiftedState=o,this.type=be}};var fe=class{constructor(o){this.status=o,this.type=ii}};var he=class{constructor(o){this.status=o,this.type=oi}};var Pt=new A$2(`@ngrx/store-devtools Options`);var Un=new A$2(`@ngrx/store-devtools Initial Config`);function ri(){return null}var Ri=`NgRx Store DevTools`;function Ni(n){let o={maxAge:!1,monitor:ri,actionSanitizer:void 0,stateSanitizer:void 0,actionCreators:void 0,name:Ri,serialize:!1,logOnly:!1,autoPause:!1,trace:!1,traceLimit:75,features:{pause:!0,lock:!0,persist:!0,export:!0,import:`custom`,jump:!0,skip:!0,reorder:!0,dispatch:!0,test:!0},connectInZone:!1},t=typeof n==`function`?n():n,e=t.logOnly?{pause:!0,export:!0,test:!0}:!1,i=t.features||e||o.features;i.import===!0&&(i.import=`custom`);let r=Object.assign({},o,{features:i},t);if(r.maxAge&&r.maxAge<2)throw new Error(`Devtools 'maxAge' cannot be less than 2, got ${r.maxAge}`);return r}function Vn(n,o){return n.filter(t=>o.indexOf(t)<0)}function ai(n){let{computedStates:o,currentStateIndex:t}=n;if(t>=o.length){let{state:i}=o[o.length-1];return i}let{state:e}=o[t];return e}function ut(n){return new it(n,+Date.now())}function Pi(n,o){return Object.keys(o).reduce((t,e)=>{let i=Number(e);return t[i]=si(n,o[i],i),t},{})}function si(n,o,t){return s(r({},o),{action:n(o.action,t)})}function Fi(n,o){return o.map((t,e)=>({state:ci(n,t.state,e),error:t.error}))}function ci(n,o,t){return n(o,t)}function li(n){return n.predicate||n.actionsSafelist||n.actionsBlocklist}function Bi(n,o,t,e){let i=[],r$1={},a=[];return n.stagedActionIds.forEach((d,f)=>{let s=n.actionsById[d];s&&(f&&ye(n.computedStates[f],s,o,t,e)||(r$1[d]=s,i.push(d),a.push(n.computedStates[f])))}),s(r({},n),{stagedActionIds:i,actionsById:r$1,computedStates:a})}function ye(n,o,t,e,i){let r=t&&!t(n,o.action),a=e&&!o.action.type.match(e.map(f=>Zn(f)).join(`|`)),d=i&&o.action.type.match(i.map(f=>Zn(f)).join(`|`));return r||a||d}function Zn(n){return n.replace(/[.*+?^${}()|[\]\\]/g,`\\$&`)}function di(n){return{ngZone:n?T(Ae$2):null,connectInZone:n}}var Ft=(()=>{class n extends E{static{this.ɵfac=(()=>{let t;return function(i){return(t||(t=iy(n)))(i||n)}})()}static{this.ɵprov=ae$2({token:n,factory:n.ɵfac})}}return n})();var Ot={START:`START`,DISPATCH:`DISPATCH`,STOP:`STOP`,ACTION:`ACTION`};var ge=new A$2(`@ngrx/store-devtools Redux Devtools Extension`);function ji(n){return typeof n==`object`&&n!==null&&!(`type`in n)&&typeof n.selected==`number`&&Array.isArray(n.args)}function Gn(n){let o=String(n),t=o.match(/^[^(]*\(([^)]*)\)/);if(!t){let e=o.match(/^\s*([^=\s(]+)\s*=>/);return e?[e[1]]:[]}return t[1].split(`,`).map(e=>e.replace(/^\s*\.{3}/,``).split(`=`)[0].trim()).filter(e=>e!==``)}function Li(n){return Array.isArray(n)?n.map(o=>({name:o.type||o.name||`anonymous`,func:o,args:Gn(o)})):Object.keys(n).map(o=>({name:o,func:n[o],args:Gn(n[o])}))}var Wn=n=>n===``?void 0:(0,eval)(`(${n})`);var mi=(()=>{class n{constructor(t,e,i){this.config=e,this.dispatcher=i,this.zoneConfig=di(this.config.connectInZone),this.devtoolsExtension=t,this.actionCreatorDescriptors=e.actionCreators?Li(e.actionCreators):void 0,this.createActionStreams()}notify(t,e){if(this.devtoolsExtension)if(t.type===pt){if(e.isLocked||e.isPaused)return;let i=ai(e);if(li(this.config)&&ye(i,t,this.config.predicate,this.config.actionsSafelist,this.config.actionsBlocklist))return;let r=this.config.stateSanitizer?ci(this.config.stateSanitizer,i,e.currentStateIndex):i,a=this.config.actionSanitizer?si(this.config.actionSanitizer,t,e.nextActionId):t;this.sendToReduxDevtools(()=>this.extensionConnection.send(a,r))}else{let i=s(r({},e),{stagedActionIds:e.stagedActionIds,actionsById:this.config.actionSanitizer?Pi(this.config.actionSanitizer,e.actionsById):e.actionsById,computedStates:this.config.stateSanitizer?Fi(this.config.stateSanitizer,e.computedStates):e.computedStates});this.sendToReduxDevtools(()=>this.devtoolsExtension.send(null,i,this.getExtensionConfig(this.config)))}}createChangesObservable(){return this.devtoolsExtension?new _(t=>{let e=this.zoneConfig.connectInZone?this.zoneConfig.ngZone.runOutsideAngular(()=>this.devtoolsExtension.connect(this.getExtensionConfig(this.config))):this.devtoolsExtension.connect(this.getExtensionConfig(this.config));return this.extensionConnection=e,e.init(),e.subscribe(i=>t.next(i)),e.unsubscribe}):$e}createActionStreams(){let t=this.createChangesObservable().pipe(is()),e=t.pipe(Yt$1(s=>s.type===Ot.START)),i=t.pipe(Yt$1(s=>s.type===Ot.STOP)),r=t.pipe(Yt$1(s=>s.type===Ot.DISPATCH),ue$1(s=>this.unwrapAction(s.payload)),mg(s=>s.type===be?this.dispatcher.pipe(Yt$1(y=>y.type===Xe$1),tg(1e3),yg(1e3),ue$1(()=>s),ns(()=>Xi$1(s)),rs(1)):Xi$1(s))),d=t.pipe(Yt$1(s=>s.type===Ot.ACTION),ue$1(s=>this.unwrapAction(s.payload))).pipe(Rg(i)),f=r.pipe(Rg(i));this.start$=e.pipe(Rg(i)),this.actions$=this.start$.pipe(Fl(()=>d)),this.liftedActions$=this.start$.pipe(Fl(()=>f))}unwrapAction(t){if(typeof t==`string`)return(0,eval)(`(${t})`);if(this.actionCreatorDescriptors&&ji(t)){let e=this.actionCreatorDescriptors[t.selected];if(e){let i=t.args.map(Wn);if(t.rest){let r=Wn(t.rest);Array.isArray(r)&&i.push(...r)}return e.func(...i)}}return t}getExtensionConfig(t){let e={name:t.name,features:t.features,serialize:t.serialize,autoPause:t.autoPause??!1,trace:t.trace??!1,traceLimit:t.traceLimit??75};return t.maxAge!==!1&&(e.maxAge=t.maxAge),this.actionCreatorDescriptors&&(e.actionCreators=this.actionCreatorDescriptors),e}sendToReduxDevtools(t){try{t()}catch(e){console.warn(`@ngrx/store-devtools: something went wrong inside the redux devtools`,e)}}static{this.ɵfac=function(e){return new(e||n)(xe(ge),xe(Pt),xe(Ft))}}static{this.ɵprov=ae$2({token:n,factory:n.ɵfac})}}return n})();var Nt={type:xe$1};var Hi={type:`@ngrx/store-devtools/recompute`};function ui(n,o,t,e,i){if(e)return{state:t,error:`Interrupted by an error up the chain`};let r=t,a;try{r=n(t,o)}catch(d){a=d.toString(),i.handleError(d)}return{state:r,error:a}}function Rt(n,o,t,e,i,r,a,d,f){if(o>=n.length&&n.length===r.length)return n;let s=n.slice(0,o),y=r.length-(f?1:0);for(let l=o;l<y;l++){let p=r[l],A=i[p].action,u=s[l-1],m=u?u.state:e,D=u?u.error:void 0,O=a.indexOf(p)>-1?u:ui(t,A,m,D,d);s.push(O)}return f&&s.push(n[n.length-1]),s}function $i(n,o){return{monitorState:o(void 0,{}),nextActionId:1,actionsById:{0:ut(Nt)},stagedActionIds:[0],skippedActionIds:[],committedState:n,currentStateIndex:0,computedStates:[],isLocked:!1,isPaused:!1}}function Ui(n,o,t,e,i={}){return r$2=>(a,d)=>{let{monitorState:f,actionsById:s$1,nextActionId:y,stagedActionIds:l,skippedActionIds:p,committedState:A,currentStateIndex:u,computedStates:m,isLocked:D,isPaused:S}=a||o;a||(s$1=Object.create(s$1));function O(b){let g=b,B=l.slice(1,g+1);for(let C=0;C<B.length;C++)if(m[C+1].error){g=C,B=l.slice(1,g+1);break}else delete s$1[B[C]];p=p.filter(C=>B.indexOf(C)===-1),l=[0,...l.slice(g+1)],A=m[g].state,m=m.slice(g),u=u>g?u-g:0}function R(){s$1={0:ut(Nt)},y=1,l=[0],p=[],A=m[u].state,u=0,m=[]}let h=0;switch(d.type){case ii:D=d.status,h=Infinity;break;case oi:S=d.status,S?(l=[...l,y],s$1[y]=new it({type:`@ngrx/devtools/pause`},+Date.now()),y++,h=l.length-1,m=m.concat(m[m.length-1]),u===l.length-2&&u++,h=Infinity):R();break;case Xn:s$1={0:ut(Nt)},y=1,l=[0],p=[],A=n,u=0,m=[];break;case Qn:R();break;case Kn:s$1={0:ut(Nt)},y=1,l=[0],p=[],u=0,m=[];break;case ti:{let{id:b}=d;p.indexOf(b)===-1?p=[b,...p]:p=p.filter(B=>B!==b),h=l.indexOf(b);break}case Oi:{let{start:b,end:g,active:B}=d,C=[];for(let Ut=b;Ut<g;Ut++)C.push(Ut);B?p=Vn(p,C):p=[...p,...C],h=l.indexOf(b);break}case ei:u=d.index,h=Infinity;break;case ni:{let b=l.indexOf(d.actionId);b!==-1&&(u=b),h=Infinity;break}case Jn:l=Vn(l,p),p=[],u=Math.min(u,l.length-1);break;case pt:{if(D)return a||o;if(S||a&&ye(a.computedStates[u],d,i.predicate,i.actionsSafelist,i.actionsBlocklist)){let g=m[m.length-1];m=[...m.slice(0,-1),ui(r$2,d.action,g.state,g.error,t)],h=Infinity;break}i.maxAge&&l.length===i.maxAge&&O(1),u===l.length-1&&u++;let b=y++;s$1[b]=d,l=[...l,b],h=l.length-1;break}case be:({monitorState:f,actionsById:s$1,nextActionId:y,stagedActionIds:l,skippedActionIds:p,committedState:A,currentStateIndex:u,computedStates:m,isLocked:D,isPaused:S}=d.nextLiftedState);break;case xe$1:h=0,i.maxAge&&l.length>i.maxAge&&(m=Rt(m,h,r$2,A,s$1,l,p,t,S),O(l.length-i.maxAge),h=Infinity);break;case Xe$1:if(m.filter(g=>g.error).length>0)h=0,i.maxAge&&l.length>i.maxAge&&(m=Rt(m,h,r$2,A,s$1,l,p,t,S),O(l.length-i.maxAge),h=Infinity);else{if(!S&&!D){u===l.length-1&&u++;let g=y++;s$1[g]=new it(d,+Date.now()),l=[...l,g],h=l.length-1,m=Rt(m,h,r$2,A,s$1,l,p,t,S)}m=m.map(g=>s(r({},g),{state:r$2(g.state,Hi)})),u=l.length-1,i.maxAge&&l.length>i.maxAge&&O(l.length-i.maxAge),h=Infinity}break;default:h=Infinity;break}return m=Rt(m,h,r$2,A,s$1,l,p,t,S),f=e(f,d),{monitorState:f,actionsById:s$1,nextActionId:y,stagedActionIds:l,skippedActionIds:p,committedState:A,currentStateIndex:u,computedStates:m,isLocked:D,isPaused:S}}}var Yn=(()=>{class n{constructor(t,e,i,r,a,d,f,s){let y=$i(f,s.monitor),l=Ui(f,y,d,s.monitor,s),p=hg(hg(e.asObservable().pipe(xg(1)),r.actions$).pipe(ue$1(ut)),t,r.liftedActions$).pipe(Pn(qh)),A=i.pipe(ue$1(l)),u=di(s.connectInZone),m=new kn(1);this.liftedStateSubscription=p.pipe(Og(A),qn(u),Sg(({state:O},[R,h])=>{let b=h(O,R);return R.type!==pt&&li(s)&&(b=Bi(b,s.predicate,s.actionsSafelist,s.actionsBlocklist)),r.notify(R,b),{state:b,action:R}},{state:y,action:null})).subscribe(({state:O,action:R})=>{if(m.next(O),R.type===pt){let h=R.action;a.next(h)}}),this.extensionStartSubscription=r.start$.pipe(qn(u)).subscribe(()=>{this.refresh()});let D=m.asObservable(),S=D.pipe(ue$1(ai));Object.defineProperty(S,"state",{value:T$1(S,{manualCleanup:!0,requireSync:!0})}),this.dispatcher=t,this.liftedState=D,this.state=S}ngOnDestroy(){this.liftedStateSubscription.unsubscribe(),this.extensionStartSubscription.unsubscribe()}dispatch(t){this.dispatcher.next(t)}next(t){this.dispatcher.next(t)}error(t){}complete(){}performAction(t){this.dispatch(new it(t,+Date.now()))}refresh(){this.dispatch(new re)}reset(){this.dispatch(new ae(+Date.now()))}rollback(){this.dispatch(new se(+Date.now()))}commit(){this.dispatch(new ce(+Date.now()))}sweep(){this.dispatch(new le)}toggleAction(t){this.dispatch(new de(t))}jumpToAction(t){this.dispatch(new ue(t))}jumpToState(t){this.dispatch(new me(t))}importState(t){this.dispatch(new pe(t))}lockChanges(t){this.dispatch(new fe(t))}pauseRecording(t){this.dispatch(new he(t))}static{this.ɵfac=function(e){return new(e||n)(xe(Ft),xe(E),xe(v),xe(mi),xe(B$1),xe(st$2),xe(q$3),xe(Pt))}}static{this.ɵprov=ae$2({token:n,factory:n.ɵfac})}}return n})();function qn({ngZone:n,connectInZone:o}){return t=>o?new _(e=>t.subscribe({next:i=>n.run(()=>e.next(i)),error:i=>n.run(()=>e.error(i)),complete:()=>n.run(()=>e.complete())})):t}var Vi=new A$2(`@ngrx/store-devtools Is Devtools Extension or Monitor Present`);function Zi(n,o){return!!n||o.monitor!==ri}function Gi(){let n=`__REDUX_DEVTOOLS_EXTENSION__`;return typeof window==`object`&&typeof window[n]<`u`?window[n]:null}function Wi(n){return n.state}function pi(n={}){return Mo([mi,Ft,Yn,{provide:Un,useValue:n},{provide:Vi,deps:[ge,Pt],useFactory:Zi},{provide:ge,useFactory:Gi},{provide:Pt,deps:[Un],useFactory:Ni},{provide:w,deps:[Yn],useFactory:Wi},{provide:b,useExisting:Ft}])}var fi=[{path:`auth`,loadChildren:()=>import(`./chunk-CWSrnMrB.js`).then(n=>n.authRoutes)},{path:`projects`,canActivate:[p],loadChildren:()=>import(`./chunk-75af1CDF2.js`).then(n=>n.projectsRoutes)},{path:`tasks`,canActivate:[p],loadChildren:()=>import(`./chunk-7w3O9hEh2.js`).then(n=>n.tasksRoutes)},{path:`team`,canActivate:[p],loadChildren:()=>import(`./chunk-BU2fY7SW2.js`).then(n=>n.teamRoutes)},{path:`dashboard`,canActivate:[p],loadChildren:()=>import(`./chunk-C1S-I2bS.js`).then(n=>n.dashboardRoutes)},{path:`reports`,canActivate:[p],loadChildren:()=>import(`./chunk-CXmm3YZR2.js`).then(n=>n.reportsRoutes)},{path:``,pathMatch:`full`,redirectTo:`dashboard`}];var $=class n{nextId=0;messages=Uo([]);notifyError(o){this.push(o,`error`)}notifyInfo(o){this.push(o,`info`)}dismiss(o){this.messages.update(t=>t.filter(e=>e.id!==o))}push(o,t){let e=this.nextId++;this.messages.update(i=>[...i,{id:e,text:o,level:t}]),setTimeout(()=>this.dismiss(e),6e3)}static ɵfac=function(t){return new(t||n)};static ɵprov=ae$2({token:n,factory:n.ɵfac,providedIn:`root`})};var Bt=null;var hi=(n$1,o)=>{let t=T($),e=T(n),i=T(Z$4);return o(n$1).pipe(ns(r=>{if(!(r instanceof Fe))return es(()=>r);if(r.status===401&&!n$1.url.includes(`/api/auth/login`)&&!n$1.url.includes(`/api/auth/refresh`))return Yi(n$1,o,e,i);if(r.status!==401){let a=r.error&&typeof r.error==`object`&&`title`in r.error?String(r.error.title):`Request failed.`;t.notifyError(a)}return es(()=>r)}))};function Yi(n,o,t,e){return Bt||(Bt=t.refresh().pipe(Pl(1),Ll(()=>{Bt=null}))),Bt.pipe(Fl(i=>{e.dispatch(an.tokenRefreshed({accessToken:i.accessToken,expiresAt:i.expiresAt,user:{id:i.user.id,fullName:i.user.fullName,email:i.user.email,role:i.user.role}}));return o(n.clone({setHeaders:{Authorization:`Bearer ${i.accessToken}`}}))}),ns(i=>(e.dispatch(an.logoutRequested()),es(()=>i))))}var gi=(n,o)=>{let e=T(Z$4).selectSignal(f.selectAccessToken)(),i=n;return e&&(i=i.clone({setHeaders:{Authorization:`Bearer ${e}`}})),i.url.startsWith(`/api/auth/`)&&(i=i.clone({withCredentials:!0})),o(i)};var jt=class n{notifications=T($);handleError(o){console.error(o),this.notifications.notifyError(`Something went wrong. Please try again.`)}static ɵfac=function(t){return new(t||n)};static ɵprov=ae$2({token:n,factory:n.ɵfac})};var Lt=class n$2{actions$=T(bt$1);authService=T(n);router=T(je$1);login$=Ct(()=>this.actions$.pipe(Dt(an.loginSubmitted),Fl(({email:o,password:t})=>this.authService.login({email:o,password:t}).pipe(ue$1(e=>an.loginSuccess({accessToken:e.accessToken,expiresAt:e.expiresAt,user:{id:e.user.id,fullName:e.user.fullName,email:e.user.email,role:e.user.role}})),ns(e=>Xi$1(an.loginFailure({error:e.error?.title??`Invalid credentials`})))))));logout$=Ct(()=>this.actions$.pipe(Dt(an.logoutRequested),Fl(()=>this.authService.logout().pipe(ue$1(()=>an.logoutCompleted()),ns(()=>Xi$1(an.logoutCompleted()))))));logoutCompleted$=Ct(()=>this.actions$.pipe(Dt(an.logoutCompleted),jl(()=>{this.router.navigateByUrl(`/auth/login`)})),{dispatch:!1});static ɵfac=function(t){return new(t||n$2)};static ɵprov=ae$2({token:n$2,factory:n$2.ɵfac})};var bi={providers:[Em(),Hn(),Zc$1(fi),cu(lu([gi,hi]),du({cookieName:`XSRF-TOKEN`,headerName:`X-XSRF-TOKEN`})),nn(),tn(f),wt$1(Lt),pi({maxAge:25,logOnly:!s1()}),{provide:st$2,useClass:jt},{provide:je,useValue:{subscriptSizing:`dynamic`}},$n()]};function qi(n,o){if(n&1){let t=rD();wi$1(0,`div`,1)(1,`button`,2),Zp(`click`,function(){Nu(t);return Su(uD().action())}),HD(2),Uc()()}if(n&2){let t=uD();Gv(2),Qc(` `,t.data.action,` `)}}var Xi=[`label`];function Ki(n,o){}var Qi=Math.pow(2,31)-1;var ft=class{_overlayRef;instance;containerInstance;_afterDismissed=new Q$1;_afterOpened=new Q$1;_onAction=new Q$1;_durationTimeoutId;_dismissedByAction=!1;constructor(o,t){this._overlayRef=t,this.containerInstance=o,o._onExit.subscribe(()=>this._finishDismiss())}dismiss(){this._afterDismissed.closed||this.containerInstance.exit(),clearTimeout(this._durationTimeoutId)}dismissWithAction(){this._onAction.closed||(this._dismissedByAction=!0,this._onAction.next(),this._onAction.complete(),this.dismiss()),clearTimeout(this._durationTimeoutId)}closeWithAction(){this.dismissWithAction()}_dismissAfter(o){this._durationTimeoutId=setTimeout(()=>this.dismiss(),Math.min(o,Qi))}_open(){this._afterOpened.closed||(this._afterOpened.next(),this._afterOpened.complete())}_finishDismiss(){this._overlayRef.dispose(),this._onAction.closed||this._onAction.complete(),this._afterDismissed.next({dismissedByAction:this._dismissedByAction}),this._afterDismissed.complete(),this._dismissedByAction=!1}afterDismissed(){return this._afterDismissed}afterOpened(){return this.containerInstance._onEnter}onAction(){return this._onAction}};var yi=new A$2(`MatSnackBarData`);var ot=class{politeness=`polite`;announcementMessage=``;viewContainerRef;duration=0;panelClass;direction;data=null;horizontalPosition=`center`;verticalPosition=`bottom`};var Ji=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵdir=ME({type:n,selectors:[[``,`matSnackBarLabel`,``]],hostAttrs:[1,`mat-mdc-snack-bar-label`,`mdc-snackbar__label`]})}return n})();var to=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵdir=ME({type:n,selectors:[[``,`matSnackBarActions`,``]],hostAttrs:[1,`mat-mdc-snack-bar-actions`,`mdc-snackbar__actions`]})}return n})();var eo=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵdir=ME({type:n,selectors:[[``,`matSnackBarAction`,``]],hostAttrs:[1,`mat-mdc-snack-bar-action`,`mdc-snackbar__action`]})}return n})();var _i=(()=>{class n{snackBarRef=T(ft);data=T(yi);action(){this.snackBarRef.dismissWithAction()}get hasAction(){return!!this.data.action}static ɵfac=function(e){return new(e||n)};static ɵcmp=wE({type:n,selectors:[[`simple-snack-bar`]],hostAttrs:[1,`mat-mdc-simple-snack-bar`],exportAs:[`matSnackBar`],decls:3,vars:2,consts:[[`matSnackBarLabel`,``],[`matSnackBarActions`,``],[`matButton`,``,`matSnackBarAction`,``,3,`click`]],template:function(e,i){e&1&&(wi$1(0,`div`,0),HD(1),Uc(),zE(2,qi,3,1,`div`,1)),e&2&&(Gv(),Qc(` `,i.data.message,`
`),Gv(),QE(i.hasAction?2:-1))},dependencies:[nr,Ji,to,eo],styles:[`.mat-mdc-simple-snack-bar {
  display: flex;
}
.mat-mdc-simple-snack-bar .mat-mdc-snack-bar-label {
  max-height: 50vh;
  overflow: auto;
}
`],encapsulation:2})}return n})();var _e=`_mat-snack-bar-enter`;var ve=`_mat-snack-bar-exit`;var no=(()=>{class n extends j{_ngZone=T(Ae$2);_elementRef=T(Dr);_changeDetectorRef=T(e1);_platform=T(l$1);_animationsDisabled=z$1();snackBarConfig=T(ot);_document=T(ir);_trackedModals=new Set;_enterFallback;_exitFallback;_injector=T(ye$1);_announceDelay=150;_announceTimeoutId;_destroyed=!1;_portalOutlet;_onAnnounce=new Q$1;_onExit=new Q$1;_onEnter=new Q$1;_animationState=`void`;_live;_label;_role;_liveElementId=T(Bt$1).getId(`mat-snack-bar-container-live-`);constructor(){super();let t=this.snackBarConfig;t.politeness===`assertive`&&!t.announcementMessage?this._live=`assertive`:t.politeness===`off`?this._live=`off`:this._live=`polite`,this._platform.FIREFOX&&(this._live===`polite`&&(this._role=`status`),this._live===`assertive`&&(this._role=`alert`))}attachComponentPortal(t){this._assertNotAttached();let e=this._portalOutlet.attachComponentPortal(t);return this._afterPortalAttached(),e}attachTemplatePortal(t){this._assertNotAttached();let e=this._portalOutlet.attachTemplatePortal(t);return this._afterPortalAttached(),e}attachDomPortal=t=>{this._assertNotAttached();let e=this._portalOutlet.attachDomPortal(t);return this._afterPortalAttached(),e};onAnimationEnd(t){t===ve?this._completeExit():t===_e&&(clearTimeout(this._enterFallback),this._ngZone.run(()=>{this._onEnter.next(),this._onEnter.complete()}))}enter(){this._destroyed||(this._animationState=`visible`,this._changeDetectorRef.markForCheck(),this._changeDetectorRef.detectChanges(),this._screenReaderAnnounce(),this._animationsDisabled?Dv(()=>{this._ngZone.run(()=>queueMicrotask(()=>this.onAnimationEnd(_e)))},{injector:this._injector}):(clearTimeout(this._enterFallback),this._enterFallback=setTimeout(()=>{this._elementRef.nativeElement.classList.add(`mat-snack-bar-fallback-visible`),this.onAnimationEnd(_e)},200)))}exit(){return this._destroyed?Xi$1(void 0):(this._ngZone.run(()=>{this._animationState=`hidden`,this._changeDetectorRef.markForCheck(),this._elementRef.nativeElement.setAttribute(`mat-exit`,``),clearTimeout(this._announceTimeoutId),this._animationsDisabled?Dv(()=>{this._ngZone.run(()=>queueMicrotask(()=>this.onAnimationEnd(ve)))},{injector:this._injector}):(clearTimeout(this._exitFallback),this._exitFallback=setTimeout(()=>this.onAnimationEnd(ve),200))}),this._onExit)}ngOnDestroy(){this._destroyed=!0,this._clearFromModals(),this._completeExit()}_completeExit(){clearTimeout(this._exitFallback),queueMicrotask(()=>{this._onExit.next(),this._onExit.complete()})}_afterPortalAttached(){let t=this._elementRef.nativeElement,e=this.snackBarConfig.panelClass;e&&(Array.isArray(e)?e.forEach(a=>t.classList.add(a)):t.classList.add(e)),this._exposeToModals();let i=this._label.nativeElement,r=`mdc-snackbar__label`;i.classList.toggle(r,!i.querySelector(`.${r}`))}_exposeToModals(){let t=this._liveElementId,e=this._document.querySelectorAll(`body > .cdk-overlay-container [aria-modal="true"]`);for(let i=0;i<e.length;i++){let r=e[i],a=r.getAttribute(`aria-owns`);this._trackedModals.add(r),a?a.indexOf(t)===-1&&r.setAttribute(`aria-owns`,a+` `+t):r.setAttribute(`aria-owns`,t)}}_clearFromModals(){this._trackedModals.forEach(t=>{let e=t.getAttribute(`aria-owns`);if(e){let i=e.replace(this._liveElementId,``).trim();i.length>0?t.setAttribute(`aria-owns`,i):t.removeAttribute(`aria-owns`)}}),this._trackedModals.clear()}_assertNotAttached(){this._portalOutlet.hasAttached()}_screenReaderAnnounce(){this._announceTimeoutId||this._ngZone.runOutsideAngular(()=>{this._announceTimeoutId=setTimeout(()=>{if(this._destroyed)return;let t=this._elementRef.nativeElement,e=t.querySelector(`[aria-hidden]`),i=t.querySelector(`[aria-live]`);if(e&&i){let r=null;this._platform.isBrowser&&document.activeElement instanceof HTMLElement&&e.contains(document.activeElement)&&(r=document.activeElement),e.removeAttribute(`aria-hidden`),i.appendChild(e),r?.focus(),this._onAnnounce.next(),this._onAnnounce.complete()}},this._announceDelay)})}static ɵfac=function(e){return new(e||n)};static ɵcmp=wE({type:n,selectors:[[`mat-snack-bar-container`]],viewQuery:function(e,i){if(e&1&&Xp(pe$1,7)(Xi,7),e&2){let r;gD(r=mD())&&(i._portalOutlet=r.first),gD(r=mD())&&(i._label=r.first)}},hostAttrs:[1,`mdc-snackbar`,`mat-mdc-snack-bar-container`],hostVars:6,hostBindings:function(e,i){e&1&&Zp(`animationend`,function(a){return i.onAnimationEnd(a.animationName)})(`animationcancel`,function(a){return i.onAnimationEnd(a.animationName)}),e&2&&ih(`mat-snack-bar-container-enter`,i._animationState===`visible`)(`mat-snack-bar-container-exit`,i._animationState===`hidden`)(`mat-snack-bar-container-animations-enabled`,!i._animationsDisabled)},features:[Op],decls:6,vars:3,consts:[[`label`,``],[1,`mdc-snackbar__surface`,`mat-mdc-snackbar-surface`],[1,`mat-mdc-snack-bar-label`],[`aria-hidden`,`true`],[`cdkPortalOutlet`,``]],template:function(e,i){e&1&&(wi$1(0,`div`,1)(1,`div`,2,0)(3,`div`,3),Pp(4,Ki,0,0,`ng-template`,4),Uc(),Up(5,`div`),Uc()()),e&2&&(Gv(5),Bp(`aria-live`,i._live)(`role`,i._role)(`id`,i._liveElementId))},dependencies:[pe$1],styles:[`@keyframes _mat-snack-bar-enter {
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
`],encapsulation:2,changeDetection:1})}return n})();var io=new A$2(`mat-snack-bar-default-options`,{providedIn:`root`,factory:()=>new ot});var oo=(()=>{class n{_live=T(Dn);_injector=T(ye$1);_breakpointObserver=T(Rt$1);_parentSnackBar=T(n,{optional:!0,skipSelf:!0});_defaultConfig=T(io);_animationsDisabled=z$1();_snackBarRefAtThisLevel=null;simpleSnackBarComponent=_i;snackBarContainerComponent=no;handsetCssClass=`mat-mdc-snack-bar-handset`;get _openedSnackBarRef(){let t=this._parentSnackBar;return t?t._openedSnackBarRef:this._snackBarRefAtThisLevel}set _openedSnackBarRef(t){this._parentSnackBar?this._parentSnackBar._openedSnackBarRef=t:this._snackBarRefAtThisLevel=t}openFromComponent(t,e){return this._attach(t,e)}openFromTemplate(t,e){return this._attach(t,e)}open(t,e=``,i){let r$3=r(r({},this._defaultConfig),i);return r$3.data={message:t,action:e},r$3.announcementMessage===t&&(r$3.announcementMessage=void 0),this.openFromComponent(this.simpleSnackBarComponent,r$3)}dismiss(){this._openedSnackBarRef&&this._openedSnackBarRef.dismiss()}ngOnDestroy(){this._snackBarRefAtThisLevel&&this._snackBarRefAtThisLevel.dismiss()}_attachSnackBarContainer(t,e){let i=e&&e.viewContainerRef&&e.viewContainerRef.injector,r=ye$1.create({parent:i||this._injector,providers:[{provide:ot,useValue:e}]}),a=new at$1(this.snackBarContainerComponent,e.viewContainerRef,r),d=t.attach(a);return d.instance.snackBarConfig=e,d.instance}_attach(t,e){let i=r(r(r({},new ot),this._defaultConfig),e),r$4=this._createOverlay(i),a=this._attachSnackBarContainer(r$4,i),d=new ft(a,r$4);if(t instanceof gr){let f=new M(t,null,{$implicit:i.data,snackBarRef:d});d.instance=a.attachTemplatePortal(f)}else{let s=new at$1(t,void 0,this._createInjector(i,d));d.instance=a.attachComponentPortal(s).instance}return this._breakpointObserver.observe(ni$2.HandsetPortrait).pipe(Rg(r$4.detachments())).subscribe(f=>{r$4.overlayElement.classList.toggle(this.handsetCssClass,f.matches)}),i.announcementMessage&&a._onAnnounce.subscribe(()=>{this._live.announce(i.announcementMessage,i.politeness)}),this._animateSnackBar(d,i),this._openedSnackBarRef=d,this._openedSnackBarRef}_animateSnackBar(t,e){t.afterDismissed().subscribe(()=>{this._openedSnackBarRef==t&&(this._openedSnackBarRef=null),e.announcementMessage&&this._live.clear()}),e.duration&&e.duration>0&&t.afterOpened().subscribe(()=>t._dismissAfter(e.duration)),this._openedSnackBarRef?(this._openedSnackBarRef.afterDismissed().subscribe(()=>{t.containerInstance.enter()}),this._openedSnackBarRef.dismiss()):t.containerInstance.enter()}_createOverlay(t){let e=new B;e.direction=t.direction;let i=$t(this._injector),r=t.direction===`rtl`,a=t.horizontalPosition===`left`||t.horizontalPosition===`start`&&!r||t.horizontalPosition===`end`&&r,d=!a&&t.horizontalPosition!==`center`;return a?i.left(`0`):d?i.right(`0`):i.centerHorizontally(),t.verticalPosition===`top`?i.top(`0`):i.bottom(`0`),e.positionStrategy=i,e.disableAnimations=this._animationsDisabled,gt$1(this._injector,e)}_createInjector(t,e){let i=t&&t.viewContainerRef&&t.viewContainerRef.injector;return ye$1.create({parent:i||this._injector,providers:[{provide:ft,useValue:e},{provide:yi,useValue:t.data}]})}static ɵfac=function(e){return new(e||n)};static ɵprov=Er({token:n,factory:n.ɵfac})}return n})();var vi=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵmod=TE({type:n});static ɵinj=tu({providers:[oo],imports:[re$1,Vt,ar,_i,St$1]})}return n})();var ro=(n,o)=>o.id;function ao(n,o){if(n&1&&(qc(0,`div`,1),HD(1),Wc()),n&2){let t=o.$implicit;ih(`notification--error`,t.level===`error`),Gv(),Qc(` `,t.text,` `)}}var zt=class n{notifications=T($);static ɵfac=function(t){return new(t||n)};static ɵcmp=wE({type:n,selectors:[[`app-notification`]],decls:2,vars:0,consts:[[1,`notification`,3,`notification--error`],[1,`notification`]],template:function(t,e){t&1&&YE(0,ao,2,3,`div`,0,ro),t&2&&KE(e.notifications.messages())},dependencies:[vi],styles:[`.notification[_ngcontent-%COMP%]{position:fixed;bottom:16px;right:16px;padding:12px 16px;border-radius:4px;background:#323232;color:#fff;margin-top:8px;z-index:1000}.notification--error[_ngcontent-%COMP%]{background:#b3261e}`]})};var so=[`*`,[[`mat-toolbar-row`]]];var co=[`*`,`mat-toolbar-row`];var lo=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵdir=ME({type:n,selectors:[[`mat-toolbar-row`]],hostAttrs:[1,`mat-toolbar-row`],exportAs:[`matToolbarRow`]})}return n})();var Si=(()=>{class n{_elementRef=T(Dr);_platform=T(l$1);_document=T(ir);color;_toolbarRows;ngAfterViewInit(){this._platform.isBrowser&&(this._checkToolbarMixedModes(),this._toolbarRows.changes.subscribe(()=>this._checkToolbarMixedModes()))}_checkToolbarMixedModes(){this._toolbarRows.length}static ɵfac=function(e){return new(e||n)};static ɵcmp=wE({type:n,selectors:[[`mat-toolbar`]],contentQueries:function(e,i,r){if(e&1&&Jp(r,lo,5),e&2){let a;gD(a=mD())&&(i._toolbarRows=a)}},hostAttrs:[1,`mat-toolbar`],hostVars:6,hostBindings:function(e,i){e&2&&(SD(i.color?`mat-`+i.color:``),ih(`mat-toolbar-multiple-rows`,i._toolbarRows.length>0)(`mat-toolbar-single-row`,i._toolbarRows.length===0))},inputs:{color:`color`},exportAs:[`matToolbar`],ngContentSelectors:co,decls:2,vars:0,template:function(e,i){e&1&&(fD(so),pD(0),pD(1,1))},styles:[`.mat-toolbar {
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
`],encapsulation:2})}return n})();var Ai=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵmod=TE({type:n});static ɵinj=tu({imports:[St$1]})}return n})();function uo(n,o){n&1&&(wi$1(0,`a`,7),HD(1,`Users`),Uc())}function po(n,o){if(n&1){let t=rD();wi$1(0,`mat-toolbar`,0)(1,`a`,1)(2,`mat-icon`),HD(3,`bolt`),Uc(),wi$1(4,`span`),HD(5,`Flowrithm`),Uc()(),wi$1(6,`nav`,2)(7,`a`,3),HD(8,`Dashboard`),Uc(),wi$1(9,`a`,4),HD(10,`Projects`),Uc(),wi$1(11,`a`,5),HD(12,`Tasks`),Uc(),wi$1(13,`a`,6),HD(14,`Reports`),Uc(),zE(15,uo,2,0,`a`,7),Uc(),Up(16,`span`,8),wi$1(17,`span`,9),HD(18),Uc(),wi$1(19,`span`,10),HD(20),Uc(),wi$1(21,`button`,11),Zp(`click`,function(){Nu(t);return Su(uD().logout())}),HD(22,`Log out`),Uc()()}if(n&2){let t=o;Gv(15),QE(t.role===`Admin`?15:-1),Gv(2),Bp(`data-role`,t.role),Gv(),dh(t.role),Gv(2),dh(t.fullName)}}var Ht=class n{store=T(Z$4);user=this.store.selectSignal(f.selectUser);logout(){this.store.dispatch(an.logoutRequested())}static ɵfac=function(t){return new(t||n)};static ɵcmp=wE({type:n,selectors:[[`app-shell-header`]],decls:1,vars:1,consts:[[`color`,`primary`,1,`shell-toolbar`],[`routerLink`,`/dashboard`,1,`brand`],[1,`shell-nav`],[`mat-button`,``,`routerLink`,`/dashboard`,`routerLinkActive`,`active-link`],[`mat-button`,``,`routerLink`,`/projects`,`routerLinkActive`,`active-link`],[`mat-button`,``,`routerLink`,`/tasks`,`routerLinkActive`,`active-link`],[`mat-button`,``,`routerLink`,`/reports`,`routerLinkActive`,`active-link`],[`mat-button`,``,`routerLink`,`/auth/admin-users`,`routerLinkActive`,`active-link`],[1,`spacer`],[1,`chip`],[1,`user-name`],[`mat-stroked-button`,``,3,`click`]],template:function(t,e){if(t&1&&zE(0,po,23,4,`mat-toolbar`,0),t&2){let i;QE((i=e.user())?0:-1,i)}},dependencies:[ir$1,Wc$1,Ai,Si,ar,nr,yt$1,wt],styles:[`.shell-toolbar[_ngcontent-%COMP%]{position:sticky;top:0;z-index:10;display:flex;flex-wrap:wrap;gap:8px 16px;row-gap:8px;height:auto;min-height:64px;padding:8px 16px}.brand[_ngcontent-%COMP%]{display:flex;align-items:center;gap:8px;color:inherit;text-decoration:none;font-weight:700;font-size:20px;letter-spacing:.2px;white-space:nowrap}.brand[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%]{color:#ffd54f}.shell-nav[_ngcontent-%COMP%]{display:flex;gap:4px;flex-wrap:wrap}.active-link[_ngcontent-%COMP%]{background:#ffffff29;border-radius:4px}.spacer[_ngcontent-%COMP%]{flex:1 1 auto}.user-name[_ngcontent-%COMP%]{white-space:nowrap}.chip[data-role][_ngcontent-%COMP%]{background:#ffffffd9}`]})};$a(class n{static ɵfac=function(t){return new(t||n)};static ɵcmp=wE({type:n,selectors:[[`app-root`]],decls:4,vars:0,consts:[[1,`app-content`]],template:function(t,e){t&1&&(Up(0,`app-shell-header`),wi$1(1,`main`,0),Up(2,`router-outlet`),Uc(),Up(3,`app-notification`))},dependencies:[fi$1,zt,Ht],encapsulation:2})},bi).catch(n=>console.error(n));export{Ke$1 as A,W$2 as B,ne$1 as C,re$1 as D,q as E,Xe as F,yt$1 as G,se$2 as H,Ye as I,Dt as J,p as K,u as L,N as M,Qe$1 as N,ut$2 as O,U$2 as P,l as Q,Ae$1 as R,j as S,pt$2 as T,we as U,re$2 as V,wt as W,n as X,bt$1 as Y,d as Z,Vt as _,U as a,dt$1 as b,li$1 as c,$$2 as d,$t as f,M as g,Kt as h,St as i,L as j,I as k,ni$1 as l,Ht$1 as m,He as n,Ve as o,B as p,f as q,Qe as r,We as s,G as t,vt as u,_t$1 as v,pe$1 as w,gt$1 as x,at$1 as y,Me as z};