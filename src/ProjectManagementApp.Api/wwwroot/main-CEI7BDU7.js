import{$ as M$1,$n as os,$t as ae$1,A as GE,An as ig,Ar as yg,At as T$1,Bt as Vo,C as Eu,Cr as vg,Ct as Rc,Dr as wt$1,Dt as Se,E as Fl,En as hg,Er as wp,F as HE,Fn as kE,Ft as Uh,Gn as ng,Ht as W$2,I as HF,In as kc,Jt as Zi$1,L as He$1,M as Gl,Mn as jl,Mr as yr,Nn as jn$1,Nt as UE,On as hi$1,Or as xE,Ot as Sp,P as Gp,Pt as UF,R as Hn$1,Rt as Vl,S as Ep,Tn as he,Tr as wg,Ut as WE,Wt as WF,X as Ln$1,Xn as oe$2,Y as Ll,Yn as nt$1,Z as Lp,Zn as og,Zt as _p,_ as Dg,_n as es,_t as Pc,b as Eg,bt as QE,cr as rg,ct as Ni$1,d as Be,dn as dE,dr as rs,dt as OE,en as ag,er as ov,fr as ru,gr as ss,gt as PF,h as Cn,hn as em,i as A$1,j as GF,jt as To,k as G$3,kr as xe,kt as Sv,lr as rm,lt as Np,m as Cm,mr as sg,mt as Op,n as $m,nn as b,nr as pr,o as AE,on as cD,p as Bp,pn as dt$3,q as La,qt as ZE,rr as q$2,rt as Mi$1,s as Ac,sn as ce$1,st as Ne,t as $F,tt as MI,ur as rr,v as Du,vn as fg,vt as Pl,w as F$1,wn as hE,wr as vr,wt as Rh,xr as ts,y as ED,yn as fr,yt as Pp,zn as lE}from"./chunk--2Z_HnF6.js";import{a as Ke,c as Mr,f as ds,g as ve,l as Ps,m as ji$1,n as Du$1,o as Le,p as er,r as Fs,s as Ls}from"./chunk-BQ8FCvSy.js";import{_ as w,a as K$2,c as Te,d as X$1,f as bn,g as vn,h as rt$1,i as J$1,l as W$3,m as mn,n as E,o as R$2,p as gn,r as In,s as Rn$1,t as D$1,u as We,v as xe$1,y as ye}from"./chunk-Dx411vT4.js";import{C as me$1,D as v,E as ue,S as le$1,T as st$2,_ as at$2,c as It$1,d as Ot$1,g as _t$1,i as Fn$1,l as J$2,m as Xn$1,n as Ds,o as Hn$2,p as Vn$1,r as Es,s as Is,t as A$2,u as Mt$1,v as b$1}from"./chunk-CqLij3A_.js";var n=class t{http=T$1(ji$1);register(e){return this.http.post(`/api/auth/register`,e)}login(e){return this.http.post(`/api/auth/login`,e,{withCredentials:!0})}logout(){return this.http.post(`/api/auth/logout`,null,{withCredentials:!0})}refresh(){return this.http.post(`/api/auth/refresh`,null,{withCredentials:!0})}static ɵfac=function(i){return new(i||t)};static ɵprov=oe$2({token:t,factory:t.ɵfac,providedIn:`root`})};var K$1={dispatch:!0,functional:!1,useEffectsErrorHandler:!0};var h=`__@ngrx/effects_create__`;function Ct(t,r={}){let e=r.functional?t:t(),n=q$2(q$2({},K$1),r);return Object.defineProperty(e,h,{value:n}),e}function Y$1(t){return Object.getOwnPropertyNames(t).filter(n=>t[n]&&t[n].hasOwnProperty(h)?t[n][h].hasOwnProperty(`dispatch`):!1).map(n=>{let s=t[n][h];return q$2({propertyName:n},s)})}function J(t){return Y$1(t)}function U$2(t){return Object.getPrototypeOf(t)}function L(t){return!!t.constructor&&t.constructor.name!==`Object`&&t.constructor.name!==`Function`}function G$2(t){return typeof t==`function`}function X(t){return t.filter(G$2)}function q$1(t,r,e){let n=U$2(t),o=!!n&&n.constructor.name!==`Object`?n.constructor.name:null;return ng(...J(t).map(({propertyName:i,dispatch:V,useEffectsErrorHandler:B})=>{let S=typeof t[i]==`function`?t[i]():t[i],M=B?e(S,r):S;return V===!1?M.pipe(sg()):M.pipe(hg()).pipe(ae$1(z=>({effect:t[i],notification:z,propertyName:i,sourceName:o,sourceInstance:t})))}))}var Q=10;function H$1(t,r,e=Q){return t.pipe(rs(n=>(r&&r.handleError(n),e<=1?t:H$1(t,r,e-1))))}var bt=(()=>{class t extends b{constructor(e){super(),e&&(this.source=e)}lift(e){let n=new t;return n.source=this,n.operator=e,n}static{this.ɵfac=function(n){return new(n||t)(Ne(J$1))}}static{this.ɵprov=oe$2({token:t,factory:t.ɵfac,providedIn:`root`})}}return t})();function Dt$1(...t){return Hn$1(r=>t.some(e=>typeof e==`string`?e===r.type:e.type===r.type))}var W$1=new A$1(`@ngrx/effects Effects Error Handler`,{providedIn:`root`,factory:()=>H$1});var tt$1=We(`@ngrx/effects/init`);function et$1(t,r){if(t.notification.kind===`N`){let e=t.notification.value;!nt(e)&&r.handleError(new Error(`Effect ${rt(t)} dispatched an invalid action: ${ot(e)}`))}}function nt(t){return typeof t!=`function`&&t&&t.type&&typeof t.type==`string`}function rt({propertyName:t,sourceInstance:r,sourceName:e}){let n=typeof r[t]==`function`;return!!e?`"${e}.${String(t)}${n?`()`:``}"`:`"${String(t)}()"`}function ot(t){try{return JSON.stringify(t)}catch{return t}}var st$1=`ngrxOnIdentifyEffects`;function it(t){return y(t,st$1)}var ft$1=`ngrxOnRunEffects`;function ct$1(t){return y(t,ft$1)}var ut$1=`ngrxOnInitEffects`;function at$1(t){return y(t,ut$1)}function y(t,r){return t&&r in t&&typeof t[r]==`function`}var k=(()=>{class t extends G$3{constructor(e,n){super(),this.errorHandler=e,this.effectsErrorHandler=n}addEffects(e){this.next(e)}toActions(){return this.pipe(fg(e=>L(e)?U$2(e):e),wt$1(e=>e.pipe(fg(dt$2))),wt$1(e=>{return ng(e.pipe(Ll(o=>lt$2(this.errorHandler,this.effectsErrorHandler)(o)),ae$1(o=>(et$1(o,this.errorHandler),o.notification)),Hn$1(o=>o.kind===`N`&&o.value!=null),ag()),e.pipe(os(1),Hn$1(at$1),ae$1(o=>o.ngrxOnInitEffects())))}))}static{this.ɵfac=function(n){return new(n||t)(Ne(nt$1),Ne(W$1))}}static{this.ɵprov=oe$2({token:t,factory:t.ɵfac,providedIn:`root`})}}return t})();function dt$2(t){return it(t)?t.ngrxOnIdentifyEffects():``}function lt$2(t,r){return e=>{let n=q$1(e,t,r);return ct$1(e)?e.ngrxOnRunEffects(n):n}}var Et=(()=>{class t{get isStarted(){return!!this.effectsSubscription}constructor(e,n){this.effectSources=e,this.store=n,this.effectsSubscription=null}start(){this.effectsSubscription||(this.effectsSubscription=this.effectSources.toActions().subscribe(this.store))}ngOnDestroy(){this.effectsSubscription&&(this.effectsSubscription.unsubscribe(),this.effectsSubscription=null)}static{this.ɵfac=function(n){return new(n||t)(Ne(k),Ne(X$1))}}static{this.ɵprov=oe$2({token:t,factory:t.ɵfac,providedIn:`root`})}}return t})();function wt(...t){let r=t.flat();return To([X(r),ru(()=>{T$1(K$2),T$1(xe$1,{optional:!0});let n=T$1(Et),s=T$1(k),o=!n.isStarted;o&&n.start();for(let c of r){let i=G$2(c)?T$1(c):c;s.addEffects(i)}o&&T$1(X$1).dispatch(tt$1())})])}function B$1(i){return i&&typeof i.connect==`function`&&!(i instanceof Zi$1)}var c=(function(i){return i[i.REPLACED=0]=`REPLACED`,i[i.INSERTED=1]=`INSERTED`,i[i.MOVED=2]=`MOVED`,i[i.REMOVED=3]=`REMOVED`,i})(c||{});var O$1=class{viewCacheSize=20;_viewCache=[];applyChanges(o,e,t,n,r){o.forEachOperation((s,a,d)=>{let h,u;if(s.previousIndex==null){let V=()=>t(s,a,d);h=this._insertView(V,d,e,n(s)),u=h?c.INSERTED:c.REPLACED}else d==null?(this._detachAndCacheView(a,e),u=c.REMOVED):(h=this._moveView(a,d,e,n(s)),u=c.MOVED);r&&r({context:h?.context,operation:u,record:s})})}detach(){for(let o of this._viewCache)o.destroy();this._viewCache=[]}_insertView(o,e,t,n){let r=this._insertViewFromCache(e,t);if(r){r.context.$implicit=n;return}let s=o();return t.createEmbeddedView(s.templateRef,s.context,s.index)}_detachAndCacheView(o,e){let t=e.detach(o);this._maybeCacheView(t,e)}_moveView(o,e,t,n){let r=t.get(o);return t.move(r,e),r.context.$implicit=n,r}_maybeCacheView(o,e){if(this._viewCache.length<this.viewCacheSize)this._viewCache.push(o);else{let t=e.indexOf(o);t===-1?o.destroy():e.remove(t)}}_insertViewFromCache(o,e){let t=this._viewCache.pop();return t&&e.insert(t,o),t||null}};var T=20;var je=(()=>{class i{_ngZone=T$1(Se);_platform=T$1(v);_renderer=T$1(pr).createRenderer(null,null);_cleanupGlobalListener;_scrolled=new G$3;_scrolledCount=0;scrollContainers=new Map;register(e){this.scrollContainers.has(e)||this.scrollContainers.set(e,e.elementScrolled().subscribe(()=>this._scrolled.next(e)))}deregister(e){let t=this.scrollContainers.get(e);t&&(t.unsubscribe(),this.scrollContainers.delete(e))}scrolled(e=T){return this._platform.isBrowser?new b(t=>{this._cleanupGlobalListener||(this._cleanupGlobalListener=this._ngZone.runOutsideAngular(()=>this._renderer.listen(`document`,`scroll`,()=>this._scrolled.next())));let n=e>0?this._scrolled.pipe(rg(e)).subscribe(t):this._scrolled.subscribe(t);return this._scrolledCount++,()=>{n.unsubscribe(),this._scrolledCount--,this._scrolledCount||(this._cleanupGlobalListener?.(),this._cleanupGlobalListener=void 0)}}):es()}ngOnDestroy(){this._cleanupGlobalListener?.(),this._cleanupGlobalListener=void 0,this.scrollContainers.forEach((e,t)=>this.deregister(t)),this._scrolled.complete()}ancestorScrolled(e,t){let n=this.getAncestorScrollContainers(e);return this.scrolled(t).pipe(Hn$1(r=>!r||n.indexOf(r)>-1))}getAncestorScrollContainers(e){let t=[];return this.scrollContainers.forEach((n,r)=>{this._targetContainsElement(r,e)&&t.push(r)}),t}_targetContainsElement(e,t){let n=A$2(t),r=e.getElementRef().nativeElement;do if(n==r)return!0;while(n=n.parentElement);return!1}static ɵfac=function(t){return new(t||i)};static ɵprov=yr({token:i,factory:i.ɵfac})}return i})();var F=20;var Ue=(()=>{class i{_platform=T$1(v);_listeners;_viewportSize=null;_change=new G$3;_document=T$1(rr);constructor(){let e=T$1(Se),t=T$1(pr).createRenderer(null,null);e.runOutsideAngular(()=>{if(this._platform.isBrowser){let n=r=>this._change.next(r);this._listeners=[t.listen(`window`,`resize`,n),t.listen(`window`,`orientationchange`,n)]}this.change().subscribe(()=>this._viewportSize=null)})}ngOnDestroy(){this._listeners?.forEach(e=>e()),this._change.complete()}getViewportSize(){this._viewportSize||this._updateViewportSize();let e={width:this._viewportSize.width,height:this._viewportSize.height};return this._platform.isBrowser||(this._viewportSize=null),e}getViewportRect(){let e=this.getViewportScrollPosition(),{width:t,height:n}=this.getViewportSize();return{top:e.top,left:e.left,bottom:e.top+n,right:e.left+t,height:n,width:t}}getViewportScrollPosition(){if(!this._platform.isBrowser)return{top:0,left:0};let e=this._document,t=this._getWindow(),n=e.documentElement,r=n.getBoundingClientRect();return{top:-r.top||e.body?.scrollTop||t.scrollY||n.scrollTop||0,left:-r.left||e.body?.scrollLeft||t.scrollX||n.scrollLeft||0}}change(e=F){return e>0?this._change.pipe(rg(e)):this._change}_getWindow(){return this._document.defaultView||window}_updateViewportSize(){let e=this._getWindow();this._viewportSize=this._platform.isBrowser?{width:e.innerWidth,height:e.innerHeight}:{width:0,height:0}}static ɵfac=function(t){return new(t||i)};static ɵprov=yr({token:i,factory:i.ɵfac})}return i})();var He=new A$1(`CDK_VIRTUAL_SCROLL_VIEWPORT`);var z$1=(()=>{class i{static ɵfac=function(t){return new(t||i)};static ɵmod=dE({type:i});static ɵinj=Gl({})}return i})();var Ze=(()=>{class i{static ɵfac=function(t){return new(t||i)};static ɵmod=dE({type:i});static ɵinj=Gl({imports:[at$2,z$1,at$2,z$1]})}return i})();var D=class{_attachedHost=null;attach(t){return this._attachedHost=t,t.attach(this)}detach(){let t=this._attachedHost;t!=null&&(this._attachedHost=null,t.detach())}get isAttached(){return this._attachedHost!=null}setAttachedHost(t){this._attachedHost=t}};var at=class extends D{component;viewContainerRef;injector;projectableNodes;bindings;directives;constructor(t,e,i,s,n,r){super(),this.component=t,this.viewContainerRef=e,this.injector=i,this.projectableNodes=s,this.bindings=n||null,this.directives=r||null}};var M=class extends D{templateRef;viewContainerRef;context;injector;constructor(t,e,i,s){super(),this.templateRef=t,this.viewContainerRef=e,this.context=i,this.injector=s}get origin(){return this.templateRef.elementRef}attach(t,e=this.context){return this.context=e,super.attach(t)}detach(){return this.context=void 0,super.detach()}};var lt$1=class extends D{element;constructor(t){super(),this.element=t instanceof vr?t.nativeElement:t}};var W=class{_attachedPortal=null;_disposeFn=null;_isDisposed=!1;hasAttached(){return!!this._attachedPortal}attach(t){if(t instanceof at)return this._attachedPortal=t,this.attachComponentPortal(t);if(t instanceof M)return this._attachedPortal=t,this.attachTemplatePortal(t);if(this.attachDomPortal&&t instanceof lt$1)return this._attachedPortal=t,this.attachDomPortal(t)}attachDomPortal=null;detach(){this._attachedPortal&&(this._attachedPortal.setAttachedHost(null),this._attachedPortal=null),this._invokeDisposeFn()}dispose(){this.hasAttached()&&this.detach(),this._invokeDisposeFn(),this._isDisposed=!0}setDisposeFn(t){this._disposeFn=t}_invokeDisposeFn(){this._disposeFn&&(this._disposeFn(),this._disposeFn=null)}};var j$1=class extends W{outletElement;_appRef;_defaultInjector;constructor(t,e,i){super(),this.outletElement=t,this._appRef=e,this._defaultInjector=i}attachComponentPortal(t){let e;if(t.viewContainerRef){let i=t.injector||t.viewContainerRef.injector,s=i.get(Cn,null,{optional:!0})||void 0;e=t.viewContainerRef.createComponent(t.component,{index:t.viewContainerRef.length,injector:i,ngModuleRef:s,projectableNodes:t.projectableNodes||void 0,bindings:t.bindings||void 0,directives:t.directives||void 0}),this.setDisposeFn(()=>e.destroy())}else{let i=this._appRef,s=t.injector||this._defaultInjector||he.NULL,n=s.get(ce$1,i.injector);e=WF(t.component,{elementInjector:s,environmentInjector:n,projectableNodes:t.projectableNodes||void 0,bindings:t.bindings||void 0,directives:t.directives||void 0}),i.attachView(e.hostView),this.setDisposeFn(()=>{i.viewCount>0&&i.detachView(e.hostView),e.destroy()})}return this.outletElement.appendChild(this._getComponentRootNode(e)),this._attachedPortal=t,e}attachTemplatePortal(t){let e=t.viewContainerRef,i=e.createEmbeddedView(t.templateRef,t.context,{injector:t.injector});return i.rootNodes.forEach(s=>this.outletElement.appendChild(s)),i.detectChanges(),this.setDisposeFn(()=>{let s=e.indexOf(i);s!==-1&&e.remove(s)}),this._attachedPortal=t,i}attachDomPortal=t=>{let e=t.element;e.parentNode;let i=this.outletElement.ownerDocument.createComment(`dom-portal`);e.parentNode.insertBefore(i,e),this.outletElement.appendChild(e),this._attachedPortal=t,super.setDisposeFn(()=>{i.parentNode&&i.parentNode.replaceChild(e,i)})};dispose(){super.dispose(),this.outletElement.remove()}_getComponentRootNode(t){return t.hostView.rootNodes[0]}};var pe$1=(()=>{class o extends W{_moduleRef=T$1(Cn,{optional:!0});_document=T$1(rr);_viewContainerRef=T$1(Mi$1);_isInitialized=!1;_attachedRef=null;get portal(){return this._attachedPortal}set portal(e){this.hasAttached()&&!e&&!this._isInitialized||(this.hasAttached()&&super.detach(),e&&super.attach(e),this._attachedPortal=e||null)}attached=new Be;get attachedRef(){return this._attachedRef}ngOnInit(){this._isInitialized=!0}ngOnDestroy(){super.dispose(),this._attachedRef=this._attachedPortal=null}attachComponentPortal(e){e.setAttachedHost(this);let i=e.viewContainerRef!=null?e.viewContainerRef:this._viewContainerRef,s=i.createComponent(e.component,{index:i.length,injector:e.injector||i.injector,projectableNodes:e.projectableNodes||void 0,ngModuleRef:this._moduleRef||void 0,bindings:e.bindings||void 0,directives:e.directives||void 0});return i!==this._viewContainerRef&&this._getRootNode().appendChild(s.hostView.rootNodes[0]),super.setDisposeFn(()=>s.destroy()),this._attachedPortal=e,this._attachedRef=s,this.attached.emit(s),s}attachTemplatePortal(e){e.setAttachedHost(this);let i=this._viewContainerRef.createEmbeddedView(e.templateRef,e.context,{injector:e.injector});return super.setDisposeFn(()=>this._viewContainerRef.clear()),this._attachedPortal=e,this._attachedRef=i,this.attached.emit(i),i}attachDomPortal=e=>{let i=e.element;i.parentNode;let s=this._document.createComment(`dom-portal`);e.setAttachedHost(this),i.parentNode.insertBefore(s,i),this._getRootNode().appendChild(i),this._attachedPortal=e,super.setDisposeFn(()=>{s.parentNode&&s.parentNode.replaceChild(i,s)})};_getRootNode(){let e=this._viewContainerRef.element.nativeElement;return e.nodeType===e.ELEMENT_NODE?e:e.parentNode}static ɵfac=(()=>{let e;return function(s){return(e||(e=$m(o)))(s||o)}})();static ɵdir=hE({type:o,selectors:[[``,`cdkPortalOutlet`,``]],inputs:{portal:[0,`cdkPortalOutlet`,`portal`]},outputs:{attached:`attached`},exportAs:[`cdkPortalOutlet`],features:[Ep]})}return o})();var Vt=(()=>{class o{static ɵfac=function(i){return new(i||o)};static ɵmod=dE({type:o});static ɵinj=Gl({})}return o})();var Nt$1=Hn$2();function Ht(o){return new z(o.get(Ue),o.get(rr))}var z=class{_viewportRuler;_previousHTMLStyles={top:``,left:``};_previousScrollPosition;_isEnabled=!1;_document;constructor(t,e){this._viewportRuler=t,this._document=e}attach(){}enable(){if(this._canBeEnabled()){let t=this._document.documentElement;this._previousScrollPosition=this._viewportRuler.getViewportScrollPosition(),this._previousHTMLStyles.left=t.style.left||``,this._previousHTMLStyles.top=t.style.top||``,t.style.left=Fn$1(-this._previousScrollPosition.left),t.style.top=Fn$1(-this._previousScrollPosition.top),t.classList.add(`cdk-global-scrollblock`),this._isEnabled=!0}}disable(){if(this._isEnabled){let t=this._document.documentElement,e=this._document.body,i=t.style,s=e.style,n=i.scrollBehavior||``,r=s.scrollBehavior||``;this._isEnabled=!1,i.left=this._previousHTMLStyles.left,i.top=this._previousHTMLStyles.top,t.classList.remove(`cdk-global-scrollblock`),Nt$1&&(i.scrollBehavior=s.scrollBehavior=`auto`),window.scroll(this._previousScrollPosition.left,this._previousScrollPosition.top),Nt$1&&(i.scrollBehavior=n,s.scrollBehavior=r)}}_canBeEnabled(){if(this._document.documentElement.classList.contains(`cdk-global-scrollblock`)||this._isEnabled)return!1;let e=this._document.documentElement,i=this._viewportRuler.getViewportSize();return e.scrollHeight>i.height||e.scrollWidth>i.width}};function Wt$1(o,t){return new Z(o.get(je),o.get(Se),o.get(Ue),t)}var Z=class{_scrollDispatcher;_ngZone;_viewportRuler;_config;_scrollSubscription=null;_overlayRef;_initialScrollPosition;constructor(t,e,i,s){this._scrollDispatcher=t,this._ngZone=e,this._viewportRuler=i,this._config=s}attach(t){this._overlayRef,this._overlayRef=t}enable(){if(this._scrollSubscription)return;let t=this._scrollDispatcher.scrolled(0).pipe(Hn$1(e=>!e||!this._overlayRef.overlayElement.contains(e.getElementRef().nativeElement)));this._config&&this._config.threshold&&this._config.threshold>1?(this._initialScrollPosition=this._viewportRuler.getViewportScrollPosition().top,this._scrollSubscription=t.subscribe(()=>{let e=this._viewportRuler.getViewportScrollPosition().top;Math.abs(e-this._initialScrollPosition)>this._config.threshold?this._detach():this._overlayRef.updatePosition()})):this._scrollSubscription=t.subscribe(this._detach)}disable(){this._scrollSubscription&&(this._scrollSubscription.unsubscribe(),this._scrollSubscription=null)}detach(){this.disable(),this._overlayRef=null}_detach=()=>{this.disable(),this._overlayRef.hasAttached()&&this._ngZone.run(()=>this._overlayRef.detach())}};var A=class{enable(){}disable(){}attach(){}};function ht(o,t){return t.some(e=>{let i=o.bottom<e.top,s=o.top>e.bottom,n=o.right<e.left,r=o.left>e.right;return i||s||n||r})}function Ft$1(o,t){return t.some(e=>{let i=o.top<e.top,s=o.bottom>e.bottom,n=o.left<e.left,r=o.right>e.right;return i||s||n||r})}function pt(o,t){return new U$1(o.get(je),o.get(Ue),o.get(Se),t)}var U$1=class{_scrollDispatcher;_viewportRuler;_ngZone;_config;_scrollSubscription=null;_overlayRef;constructor(t,e,i,s){this._scrollDispatcher=t,this._viewportRuler=e,this._ngZone=i,this._config=s}attach(t){this._overlayRef,this._overlayRef=t}enable(){if(!this._scrollSubscription){let t=this._config?this._config.scrollThrottle:0;this._scrollSubscription=this._scrollDispatcher.scrolled(t).subscribe(()=>{if(this._overlayRef.updatePosition(),this._config&&this._config.autoClose){let e=this._overlayRef.overlayElement.getBoundingClientRect(),{width:i,height:s}=this._viewportRuler.getViewportSize();ht(e,[{width:i,height:s,bottom:s,right:i,top:0,left:0}])&&(this.disable(),this._ngZone.run(()=>this._overlayRef.detach()))}})}}disable(){this._scrollSubscription&&(this._scrollSubscription.unsubscribe(),this._scrollSubscription=null)}detach(){this.disable(),this._overlayRef=null}};var jt$1=(()=>{class o{_injector=T$1(he);noop=()=>new A;close=e=>Wt$1(this._injector,e);block=()=>Ht(this._injector);reposition=e=>pt(this._injector,e);static ɵfac=function(i){return new(i||o)};static ɵprov=yr({token:o,factory:o.ɵfac})}return o})();var B=class{positionStrategy;scrollStrategy=new A;panelClass=``;hasBackdrop=!1;backdropClass=`cdk-overlay-dark-backdrop`;disableAnimations;width;height;minWidth;minHeight;maxWidth;maxHeight;direction;disposeOnNavigation=!1;usePopover;eventPredicate;constructor(t){if(t){let e=Object.keys(t);for(let i of e)t[i]!==void 0&&(this[i]=t[i])}}};var G$1=class{connectionPair;scrollableViewProperties;constructor(t,e){this.connectionPair=t,this.scrollableViewProperties=e}};var zt=(()=>{class o{_attachedOverlays=[];_document=T$1(rr);_isAttached=!1;ngOnDestroy(){this.detach()}add(e){this.remove(e),this._attachedOverlays.push(e)}remove(e){let i=this._attachedOverlays.indexOf(e);i>-1&&this._attachedOverlays.splice(i,1),this._attachedOverlays.length===0&&this.detach()}canReceiveEvent(e,i,s){return s.observers.length<1?!1:e.eventPredicate?e.eventPredicate(i):!0}static ɵfac=function(i){return new(i||o)};static ɵprov=yr({token:o,factory:o.ɵfac})}return o})();var Zt=(()=>{class o extends zt{_ngZone=T$1(Se);_renderer=T$1(pr).createRenderer(null,null);_cleanupKeydown;add(e){super.add(e),this._isAttached||(this._ngZone.runOutsideAngular(()=>{this._cleanupKeydown=this._renderer.listen(`body`,`keydown`,this._keydownListener)}),this._isAttached=!0)}detach(){this._isAttached&&(this._cleanupKeydown?.(),this._isAttached=!1)}_keydownListener=e=>{let i=this._attachedOverlays;for(let s=i.length-1;s>-1;s--){let n=i[s];if(this.canReceiveEvent(n,e,n._keydownEvents)){this._ngZone.run(()=>n._keydownEvents.next(e));break}}};static ɵfac=function(i){return new(i||o)};static ɵprov=yr({token:o,factory:o.ɵfac})}return o})();var Ut=(()=>{class o extends zt{_platform=T$1(v);_ngZone=T$1(Se);_renderer=T$1(pr).createRenderer(null,null);_cursorOriginalValue;_cursorStyleIsSet=!1;_pointerDownEventTarget=null;_cleanups;add(e){if(super.add(e),!this._isAttached){let i=this._document.body,s={capture:!0},n=this._renderer;this._cleanups=this._ngZone.runOutsideAngular(()=>[n.listen(i,`pointerdown`,this._pointerDownListener,s),n.listen(i,`click`,this._clickListener,s),n.listen(i,`auxclick`,this._clickListener,s),n.listen(i,`contextmenu`,this._clickListener,s)]),this._platform.IOS&&!this._cursorStyleIsSet&&(this._cursorOriginalValue=i.style.cursor,i.style.cursor=`pointer`,this._cursorStyleIsSet=!0),this._isAttached=!0}}detach(){this._isAttached&&(this._cleanups?.forEach(e=>e()),this._cleanups=void 0,this._platform.IOS&&this._cursorStyleIsSet&&(this._document.body.style.cursor=this._cursorOriginalValue,this._cursorStyleIsSet=!1),this._isAttached=!1)}_pointerDownListener=e=>{this._pointerDownEventTarget=b$1(e)};_clickListener=e=>{let i=b$1(e),s=e.type===`click`&&this._pointerDownEventTarget?this._pointerDownEventTarget:i;this._pointerDownEventTarget=null;let n=this._attachedOverlays.slice();for(let r=n.length-1;r>-1;r--){let a=n[r],h=a._outsidePointerEvents;if(!(!a.hasAttached()||!this.canReceiveEvent(a,e,h))){if(Yt$1(a.overlayElement,i)||Yt$1(a.overlayElement,s))break;this._ngZone?this._ngZone.run(()=>h.next(e)):h.next(e)}}};static ɵfac=function(i){return new(i||o)};static ɵprov=yr({token:o,factory:o.ɵfac})}return o})();function Yt$1(o,t){let e=typeof ShadowRoot<`u`&&ShadowRoot,i=t;for(;i;){if(i===o)return!0;i=e&&i instanceof ShadowRoot?i.host:i.parentNode}return!1}var Gt=(()=>{class o{static ɵfac=function(i){return new(i||o)};static ɵcmp=lE({type:o,selectors:[[`ng-component`]],hostAttrs:[`cdk-overlay-style-loader`,``],decls:0,vars:0,template:function(i,s){},styles:[`.cdk-overlay-container, .cdk-global-overlay-wrapper {
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
`],encapsulation:2})}return o})();var Kt=(()=>{class o{_platform=T$1(v);_containerElement;_document=T$1(rr);_styleLoader=T$1(le$1);ngOnDestroy(){this._containerElement?.remove()}getContainerElement(){return this._loadStyles(),this._containerElement||this._createContainer(),this._containerElement}_createContainer(){let e=`cdk-overlay-container`;if(this._platform.isBrowser||Vn$1()){let s=this._document.querySelectorAll(`.${e}[platform="server"], .${e}[platform="test"]`);for(let n=0;n<s.length;n++)s[n].remove()}let i=this._document.createElement(`div`);i.classList.add(e),Vn$1()?i.setAttribute(`platform`,`test`):this._platform.isBrowser||i.setAttribute(`platform`,`server`),this._document.body.appendChild(i),this._containerElement=i}_loadStyles(){this._styleLoader.load(Gt)}static ɵfac=function(i){return new(i||o)};static ɵprov=yr({token:o,factory:o.ɵfac})}return o})();var ct=class{_renderer;_ngZone;element;_cleanupClick;_cleanupTransitionEnd;_fallbackTimeout;constructor(t,e,i,s){this._renderer=e,this._ngZone=i,this.element=t.createElement(`div`),this.element.classList.add(`cdk-overlay-backdrop`),this._cleanupClick=e.listen(this.element,`click`,s)}detach(){this._ngZone.runOutsideAngular(()=>{let t=this.element;clearTimeout(this._fallbackTimeout),this._cleanupTransitionEnd?.(),this._cleanupTransitionEnd=this._renderer.listen(t,`transitionend`,this.dispose),this._fallbackTimeout=setTimeout(this.dispose,500),t.style.pointerEvents=`none`,t.classList.remove(`cdk-overlay-backdrop-showing`)})}dispose=()=>{clearTimeout(this._fallbackTimeout),this._cleanupClick?.(),this._cleanupTransitionEnd?.(),this._cleanupClick=this._cleanupTransitionEnd=this._fallbackTimeout=void 0,this.element.remove()}};function ft(o){return o&&o.nodeType===1}var K=class{_portalOutlet;_host;_pane;_config;_ngZone;_keyboardDispatcher;_document;_location;_outsideClickDispatcher;_animationsDisabled;_injector;_renderer;_backdropClick=new G$3;_attachments=new G$3;_detachments=new G$3;_positionStrategy;_scrollStrategy;_locationChanges=F$1.EMPTY;_backdropRef=null;_detachContentMutationObserver;_detachContentAfterRenderRef;_disposed=!1;_previousHostParent;_keydownEvents=new G$3;_outsidePointerEvents=new G$3;_afterNextRenderRef;constructor(t,e,i,s,n,r,a,h,p,l=!1,d,u){this._portalOutlet=t,this._host=e,this._pane=i,this._config=s,this._ngZone=n,this._keyboardDispatcher=r,this._document=a,this._location=h,this._outsideClickDispatcher=p,this._animationsDisabled=l,this._injector=d,this._renderer=u,s.scrollStrategy&&(this._scrollStrategy=s.scrollStrategy,this._scrollStrategy.attach(this)),this._positionStrategy=s.positionStrategy}get overlayElement(){return this._pane}get backdropElement(){return this._backdropRef?.element||null}get hostElement(){return this._host}get eventPredicate(){return this._config?.eventPredicate||null}attach(t){if(this._disposed)return null;this._attachHost();let e=this._portalOutlet.attach(t);return this._positionStrategy?.attach(this),this._updateStackingOrder(),this._updateElementSize(),this._updateElementDirection(),this._scrollStrategy&&this._scrollStrategy.enable(),this._afterNextRenderRef?.destroy(),this._afterNextRenderRef=ov(()=>{this.hasAttached()&&this.updatePosition()},{injector:this._injector}),this._togglePointerEvents(!0),this._config.hasBackdrop&&this._attachBackdrop(),this._config.panelClass&&this._toggleClasses(this._pane,this._config.panelClass,!0),this._attachments.next(),this._completeDetachContent(),this._keyboardDispatcher.add(this),this._config.disposeOnNavigation&&(this._locationChanges=this._location.subscribe(()=>this.dispose())),this._outsideClickDispatcher.add(this),typeof e?.onDestroy==`function`&&e.onDestroy(()=>{this.hasAttached()&&this._ngZone.runOutsideAngular(()=>Promise.resolve().then(()=>this.detach()))}),e}detach(){if(!this.hasAttached())return;this.detachBackdrop(),this._togglePointerEvents(!1),this._positionStrategy&&this._positionStrategy.detach&&this._positionStrategy.detach(),this._scrollStrategy&&this._scrollStrategy.disable();let t=this._portalOutlet.detach();return this._detachments.next(),this._completeDetachContent(),this._keyboardDispatcher.remove(this),this._detachContentWhenEmpty(),this._locationChanges.unsubscribe(),this._outsideClickDispatcher.remove(this),t}dispose(){if(this._disposed)return;let t=this.hasAttached();this._positionStrategy&&this._positionStrategy.dispose(),this._disposeScrollStrategy(),this._backdropRef?.dispose(),this._locationChanges.unsubscribe(),this._keyboardDispatcher.remove(this),this._portalOutlet.dispose(),this._attachments.complete(),this._backdropClick.complete(),this._keydownEvents.complete(),this._outsidePointerEvents.complete(),this._outsideClickDispatcher.remove(this),this._host?.remove(),this._afterNextRenderRef?.destroy(),this._previousHostParent=this._pane=this._host=this._backdropRef=null,t&&this._detachments.next(),this._detachments.complete(),this._completeDetachContent(),this._disposed=!0}hasAttached(){return this._portalOutlet.hasAttached()}backdropClick(){return this._backdropClick}attachments(){return this._attachments}detachments(){return this._detachments}keydownEvents(){return this._keydownEvents}outsidePointerEvents(){return this._outsidePointerEvents}getConfig(){return this._config}updatePosition(){this._positionStrategy&&this._positionStrategy.apply()}updatePositionStrategy(t){t!==this._positionStrategy&&(this._positionStrategy&&this._positionStrategy.dispose(),this._positionStrategy=t,this.hasAttached()&&(t.attach(this),this.updatePosition()))}updateSize(t){this._config=q$2(q$2({},this._config),t),this._updateElementSize()}setDirection(t){this._config=W$2(q$2({},this._config),{direction:t}),this._updateElementDirection()}addPanelClass(t){this._pane&&this._toggleClasses(this._pane,t,!0)}removePanelClass(t){this._pane&&this._toggleClasses(this._pane,t,!1)}getDirection(){let t=this._config.direction;return t?typeof t==`string`?t:t.value:`ltr`}updateScrollStrategy(t){t!==this._scrollStrategy&&(this._disposeScrollStrategy(),this._scrollStrategy=t,this.hasAttached()&&(t.attach(this),t.enable()))}_updateElementDirection(){this._host.setAttribute(`dir`,this.getDirection())}_updateElementSize(){if(!this._pane)return;let t=this._pane.style;t.width=Fn$1(this._config.width),t.height=Fn$1(this._config.height),t.minWidth=Fn$1(this._config.minWidth),t.minHeight=Fn$1(this._config.minHeight),t.maxWidth=Fn$1(this._config.maxWidth),t.maxHeight=Fn$1(this._config.maxHeight)}_togglePointerEvents(t){this._pane.style.pointerEvents=t?``:`none`}_attachHost(){if(!this._host.parentElement){let t=this._config.usePopover?this._positionStrategy?.getPopoverInsertionPoint?.():null;ft(t)?t.after(this._host):t?.type===`parent`?t.element.appendChild(this._host):this._previousHostParent?.appendChild(this._host)}if(this._config.usePopover)try{this._host.showPopover()}catch{}}_attachBackdrop(){let t=`cdk-overlay-backdrop-showing`;this._backdropRef?.dispose(),this._backdropRef=new ct(this._document,this._renderer,this._ngZone,e=>{this._backdropClick.next(e)}),this._animationsDisabled&&this._backdropRef.element.classList.add(`cdk-overlay-backdrop-noop-animation`),this._config.backdropClass&&this._toggleClasses(this._backdropRef.element,this._config.backdropClass,!0),this._config.usePopover?this._host.prepend(this._backdropRef.element):this._host.parentElement.insertBefore(this._backdropRef.element,this._host),!this._animationsDisabled&&typeof requestAnimationFrame<`u`?this._ngZone.runOutsideAngular(()=>{requestAnimationFrame(()=>this._backdropRef?.element.classList.add(t))}):this._backdropRef.element.classList.add(t)}_updateStackingOrder(){!this._config.usePopover&&this._host.nextSibling&&this._host.parentNode.appendChild(this._host)}detachBackdrop(){this._animationsDisabled?(this._backdropRef?.dispose(),this._backdropRef=null):this._backdropRef?.detach()}_toggleClasses(t,e,i){let s=ue(e||[]).filter(n=>!!n);s.length&&(i?t.classList.add(...s):t.classList.remove(...s))}_detachContentWhenEmpty(){let t=!1;try{this._detachContentAfterRenderRef=ov(()=>{t=!0,this._detachContent()},{injector:this._injector})}catch(e){if(t)throw e;this._detachContent()}globalThis.MutationObserver&&this._pane&&(this._detachContentMutationObserver||=new globalThis.MutationObserver(()=>{this._detachContent()}),this._detachContentMutationObserver.observe(this._pane,{childList:!0}))}_detachContent(){(!this._pane||!this._host||this._pane.children.length===0)&&(this._pane&&this._config.panelClass&&this._toggleClasses(this._pane,this._config.panelClass,!1),this._host&&this._host.parentElement&&(this._previousHostParent=this._host.parentElement,this._host.remove()),this._completeDetachContent())}_completeDetachContent(){this._detachContentAfterRenderRef?.destroy(),this._detachContentAfterRenderRef=void 0,this._detachContentMutationObserver?.disconnect()}_disposeScrollStrategy(){let t=this._scrollStrategy;t?.disable(),t?.detach?.()}};var Tt$1=`cdk-overlay-connected-position-bounding-box`;var ee$1=/([A-Za-z%]+)$/;function ut(o,t){return new $(t,o.get(Ue),o.get(rr),o.get(v),o.get(Kt))}var $=class{_viewportRuler;_document;_platform;_overlayContainer;_overlayRef;_isInitialRender=!1;_lastBoundingBoxSize={width:0,height:0};_isPushed=!1;_canPush=!0;_growAfterOpen=!1;_hasFlexibleDimensions=!0;_positionLocked=!1;_originRect;_overlayRect;_viewportRect;_containerRect;_viewportMargin=0;_scrollables=[];_preferredPositions=[];_origin;_pane;_isDisposed=!1;_boundingBox=null;_lastPosition=null;_lastScrollVisibility=null;_positionChanges=new G$3;_resizeSubscription=F$1.EMPTY;_offsetX=0;_offsetY=0;_transformOriginSelector;_appliedPanelClasses=[];_previousPushAmount=null;_popoverLocation=`global`;positionChanges=this._positionChanges;get positions(){return this._preferredPositions}constructor(t,e,i,s,n){this._viewportRuler=e,this._document=i,this._platform=s,this._overlayContainer=n,this.setOrigin(t)}attach(t){this._overlayRef&&this._overlayRef,this._validatePositions(),t.hostElement.classList.add(Tt$1),this._overlayRef=t,this._boundingBox=t.hostElement,this._pane=t.overlayElement,this._isDisposed=!1,this._isInitialRender=!0,this._lastPosition=null,this._resizeSubscription.unsubscribe(),this._resizeSubscription=this._viewportRuler.change().subscribe(()=>{this._isInitialRender=!0,this.apply()})}apply(){if(this._isDisposed||!this._platform.isBrowser)return;if(!this._isInitialRender&&this._positionLocked&&this._lastPosition){this.reapplyLastPosition();return}this._clearPanelClasses(),this._resetOverlayElementStyles(),this._resetBoundingBoxStyles(),this._viewportRect=this._getNarrowedViewportRect(),this._originRect=this._getOriginRect(),this._overlayRect=this._pane.getBoundingClientRect(),this._containerRect=this._getContainerRect();let t=this._originRect,e=this._overlayRect,i=this._viewportRect,s=this._containerRect,n=[],r;for(let a of this._preferredPositions){let h=this._getOriginPoint(t,s,a),p=this._getOverlayPoint(h,e,a),l=this._getOverlayFit(p,e,i,a);if(l.isCompletelyWithinViewport){this._isPushed=!1,this._applyPosition(a,h);return}if(this._canFitWithFlexibleDimensions(l,p,i)){n.push({position:a,origin:h,overlayRect:e,boundingBoxRect:this._calculateBoundingBoxRect(h,a)});continue}(!r||r.overlayFit.visibleArea<l.visibleArea)&&(r={overlayFit:l,overlayPoint:p,originPoint:h,position:a,overlayRect:e})}if(n.length){let a=null,h=-1;for(let p of n){let l=p.boundingBoxRect.width*p.boundingBoxRect.height*(p.position.weight||1);l>h&&(h=l,a=p)}this._isPushed=!1,this._applyPosition(a.position,a.origin);return}if(this._canPush){this._isPushed=!0,this._applyPosition(r.position,r.originPoint);return}this._applyPosition(r.position,r.originPoint)}detach(){this._clearPanelClasses(),this._lastPosition=null,this._previousPushAmount=null,this._resizeSubscription.unsubscribe()}dispose(){this._isDisposed||(this._boundingBox&&R$1(this._boundingBox.style,{top:``,left:``,right:``,bottom:``,height:``,width:``,alignItems:``,justifyContent:``}),this._pane&&this._resetOverlayElementStyles(),this._overlayRef&&this._overlayRef.hostElement.classList.remove(Tt$1),this.detach(),this._positionChanges.complete(),this._overlayRef=this._boundingBox=null,this._isDisposed=!0)}reapplyLastPosition(){if(this._isDisposed||!this._platform.isBrowser)return;let t=this._lastPosition;t?(this._originRect=this._getOriginRect(),this._overlayRect=this._pane.getBoundingClientRect(),this._viewportRect=this._getNarrowedViewportRect(),this._containerRect=this._getContainerRect(),this._applyPosition(t,this._getOriginPoint(this._originRect,this._containerRect,t))):this.apply()}withScrollableContainers(t){return this._scrollables=t,this}withPositions(t){return this._preferredPositions=t,t.indexOf(this._lastPosition)===-1&&(this._lastPosition=null),this._validatePositions(),this}withViewportMargin(t){return this._viewportMargin=t,this}withFlexibleDimensions(t=!0){return this._hasFlexibleDimensions=t,this}withGrowAfterOpen(t=!0){return this._growAfterOpen=t,this}withPush(t=!0){return this._canPush=t,this}withLockedPosition(t=!0){return this._positionLocked=t,this}setOrigin(t){return this._origin=t,this}withDefaultOffsetX(t){return this._offsetX=t,this}withDefaultOffsetY(t){return this._offsetY=t,this}withTransformOriginOn(t){return this._transformOriginSelector=t,this}withPopoverLocation(t){return this._popoverLocation=t,this}getPopoverInsertionPoint(){return this._popoverLocation===`global`?null:this._popoverLocation!==`inline`?this._popoverLocation:this._origin instanceof vr?this._origin.nativeElement:ft(this._origin)?this._origin:null}_getOriginPoint(t,e,i){let s;if(i.originX==`center`)s=t.left+t.width/2;else{let r=this._isRtl()?t.right:t.left,a=this._isRtl()?t.left:t.right;s=i.originX==`start`?r:a}e.left<0&&(s-=e.left);let n;return i.originY==`center`?n=t.top+t.height/2:n=i.originY==`top`?t.top:t.bottom,e.top<0&&(n-=e.top),{x:s,y:n}}_getOverlayPoint(t,e,i){let s;i.overlayX==`center`?s=-e.width/2:i.overlayX===`start`?s=this._isRtl()?-e.width:0:s=this._isRtl()?0:-e.width;let n;return i.overlayY==`center`?n=-e.height/2:n=i.overlayY==`top`?0:-e.height,{x:t.x+s,y:t.y+n}}_getOverlayFit(t,e,i,s){let n=Xt(e),{x:r,y:a}=t,h=this._getOffset(s,`x`),p=this._getOffset(s,`y`);h&&(r+=h),p&&(a+=p);let l=0-r,d=r+n.width-i.width,u=0-a,g=a+n.height-i.height,_=this._subtractOverflows(n.width,l,d),v=this._subtractOverflows(n.height,u,g),vt=_*v;return{visibleArea:vt,isCompletelyWithinViewport:n.width*n.height===vt,fitsInViewportVertically:v===n.height,fitsInViewportHorizontally:_==n.width}}_canFitWithFlexibleDimensions(t,e,i){if(this._hasFlexibleDimensions){let s=i.bottom-e.y,n=i.right-e.x,r=Lt(this._overlayRef.getConfig().minHeight),a=Lt(this._overlayRef.getConfig().minWidth),h=t.fitsInViewportVertically||r!=null&&r<=s,p=t.fitsInViewportHorizontally||a!=null&&a<=n;return h&&p}return!1}_pushOverlayOnScreen(t,e,i){if(this._previousPushAmount&&this._positionLocked)return{x:t.x+this._previousPushAmount.x,y:t.y+this._previousPushAmount.y};let s=Xt(e),n=this._viewportRect,r=Math.max(t.x+s.width-n.width,0),a=Math.max(t.y+s.height-n.height,0),h=Math.max(n.top-i.top-t.y,0),p=Math.max(n.left-i.left-t.x,0),l=0,d=0;return s.width<=n.width?l=p||-r:l=t.x<this._getViewportMarginStart()?n.left-i.left-t.x:0,s.height<=n.height?d=h||-a:d=t.y<this._getViewportMarginTop()?n.top-i.top-t.y:0,this._previousPushAmount={x:l,y:d},{x:t.x+l,y:t.y+d}}_applyPosition(t,e){if(this._setTransformOrigin(t),this._setOverlayElementStyles(e,t),this._setBoundingBoxStyles(e,t),t.panelClass&&this._addPanelClasses(t.panelClass),this._positionChanges.observers.length){let i=this._getScrollVisibility();if(t!==this._lastPosition||!this._lastScrollVisibility||!ie$1(this._lastScrollVisibility,i)){let s=new G$1(t,i);this._positionChanges.next(s)}this._lastScrollVisibility=i}this._lastPosition=t,this._isInitialRender=!1}_setTransformOrigin(t){if(!this._transformOriginSelector)return;let e=this._boundingBox.querySelectorAll(this._transformOriginSelector),i,s=t.overlayY;t.overlayX===`center`?i=`center`:this._isRtl()?i=t.overlayX===`start`?`right`:`left`:i=t.overlayX===`start`?`left`:`right`;for(let n=0;n<e.length;n++)e[n].style.transformOrigin=`${i} ${s}`}_calculateBoundingBoxRect(t,e){let i=this._viewportRect,s=this._isRtl(),n,r,a;if(e.overlayY===`top`)r=t.y,n=i.height-r+this._getViewportMarginBottom();else if(e.overlayY===`bottom`)a=i.height-t.y+this._getViewportMarginTop()+this._getViewportMarginBottom(),n=i.height-a+this._getViewportMarginTop();else{let g=Math.min(i.bottom-t.y+i.top,t.y),_=this._lastBoundingBoxSize.height;n=g*2,r=t.y-g,n>_&&!this._isInitialRender&&!this._growAfterOpen&&(r=t.y-_/2)}let h=e.overlayX===`start`&&!s||e.overlayX===`end`&&s,p=e.overlayX===`end`&&!s||e.overlayX===`start`&&s,l,d,u;if(p)u=i.width-t.x+this._getViewportMarginStart()+this._getViewportMarginEnd(),l=t.x-this._getViewportMarginStart();else if(h)d=t.x,l=i.right-t.x-this._getViewportMarginEnd();else{let g=Math.min(i.right-t.x+i.left,t.x),_=this._lastBoundingBoxSize.width;l=g*2,d=t.x-g,l>_&&!this._isInitialRender&&!this._growAfterOpen&&(d=t.x-_/2)}return{top:r,left:d,bottom:a,right:u,width:l,height:n}}_setBoundingBoxStyles(t,e){let i=this._calculateBoundingBoxRect(t,e);!this._isInitialRender&&!this._growAfterOpen&&(i.height=Math.min(i.height,this._lastBoundingBoxSize.height),i.width=Math.min(i.width,this._lastBoundingBoxSize.width));let s={};if(this._hasExactPosition())s.top=s.left=`0`,s.bottom=s.right=`auto`,s.maxHeight=s.maxWidth=``,s.width=s.height=`100%`;else{let n=this._overlayRef.getConfig().maxHeight,r=this._overlayRef.getConfig().maxWidth;s.width=Fn$1(i.width),s.height=Fn$1(i.height),s.top=Fn$1(i.top)||`auto`,s.bottom=Fn$1(i.bottom)||`auto`,s.left=Fn$1(i.left)||`auto`,s.right=Fn$1(i.right)||`auto`,e.overlayX===`center`?s.alignItems=`center`:s.alignItems=e.overlayX===`end`?`flex-end`:`flex-start`,e.overlayY===`center`?s.justifyContent=`center`:s.justifyContent=e.overlayY===`bottom`?`flex-end`:`flex-start`,n&&(s.maxHeight=Fn$1(n)),r&&(s.maxWidth=Fn$1(r))}this._lastBoundingBoxSize=i,R$1(this._boundingBox.style,s)}_resetBoundingBoxStyles(){R$1(this._boundingBox.style,{top:`0`,left:`0`,right:`0`,bottom:`0`,height:``,width:``,alignItems:``,justifyContent:``})}_resetOverlayElementStyles(){R$1(this._pane.style,{top:``,left:``,bottom:``,right:``,position:``,transform:``})}_setOverlayElementStyles(t,e){let i={},s=this._hasExactPosition(),n=this._hasFlexibleDimensions,r=this._overlayRef.getConfig();if(s){let l=this._viewportRuler.getViewportScrollPosition();R$1(i,this._getExactOverlayY(e,t,l)),R$1(i,this._getExactOverlayX(e,t,l))}else i.position=`static`;let a=``,h=this._getOffset(e,`x`),p=this._getOffset(e,`y`);h&&(a+=`translateX(${h}px) `),p&&(a+=`translateY(${p}px)`),i.transform=a.trim(),r.maxHeight&&(s?i.maxHeight=Fn$1(r.maxHeight):n&&(i.maxHeight=``)),r.maxWidth&&(s?i.maxWidth=Fn$1(r.maxWidth):n&&(i.maxWidth=``)),R$1(this._pane.style,i)}_getExactOverlayY(t,e,i){let s={top:``,bottom:``},n=this._getOverlayPoint(e,this._overlayRect,t);if(this._isPushed&&(n=this._pushOverlayOnScreen(n,this._overlayRect,i)),t.overlayY===`bottom`)s.bottom=`${this._document.documentElement.clientHeight-(n.y+this._overlayRect.height)}px`;else s.top=Fn$1(n.y);return s}_getExactOverlayX(t,e,i){let s={left:``,right:``},n=this._getOverlayPoint(e,this._overlayRect,t);this._isPushed&&(n=this._pushOverlayOnScreen(n,this._overlayRect,i));let r;if(this._isRtl()?r=t.overlayX===`end`?`left`:`right`:r=t.overlayX===`end`?`right`:`left`,r===`right`)s.right=`${this._document.documentElement.clientWidth-(n.x+this._overlayRect.width)}px`;else s.left=Fn$1(n.x);return s}_getScrollVisibility(){let t=this._getOriginRect(),e=this._pane.getBoundingClientRect(),i=this._scrollables.map(s=>s.getElementRef().nativeElement.getBoundingClientRect());return{isOriginClipped:Ft$1(t,i),isOriginOutsideView:ht(t,i),isOverlayClipped:Ft$1(e,i),isOverlayOutsideView:ht(e,i)}}_subtractOverflows(t,...e){return e.reduce((i,s)=>i-Math.max(s,0),t)}_getNarrowedViewportRect(){let t=this._document.documentElement.clientWidth,e=this._document.documentElement.clientHeight,i=this._viewportRuler.getViewportScrollPosition();return{top:i.top+this._getViewportMarginTop(),left:i.left+this._getViewportMarginStart(),right:i.left+t-this._getViewportMarginEnd(),bottom:i.top+e-this._getViewportMarginBottom(),width:t-this._getViewportMarginStart()-this._getViewportMarginEnd(),height:e-this._getViewportMarginTop()-this._getViewportMarginBottom()}}_isRtl(){return this._overlayRef.getDirection()===`rtl`}_hasExactPosition(){return!this._hasFlexibleDimensions||this._isPushed}_getOffset(t,e){return e===`x`?t.offsetX==null?this._offsetX:t.offsetX:t.offsetY==null?this._offsetY:t.offsetY}_validatePositions(){}_addPanelClasses(t){this._pane&&ue(t).forEach(e=>{e!==``&&this._appliedPanelClasses.indexOf(e)===-1&&(this._appliedPanelClasses.push(e),this._pane.classList.add(e))})}_clearPanelClasses(){this._pane&&(this._appliedPanelClasses.forEach(t=>{this._pane.classList.remove(t)}),this._appliedPanelClasses=[])}_getViewportMarginStart(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.start??0}_getViewportMarginEnd(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.end??0}_getViewportMarginTop(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.top??0}_getViewportMarginBottom(){return typeof this._viewportMargin==`number`?this._viewportMargin:this._viewportMargin?.bottom??0}_getOriginRect(){let t=this._origin;if(t instanceof vr)return t.nativeElement.getBoundingClientRect();if(t instanceof Element)return t.getBoundingClientRect();let e=t.width||0,i=t.height||0;return{top:t.y,bottom:t.y+i,left:t.x,right:t.x+e,height:i,width:e}}_getContainerRect(){let t=this._overlayRef.getConfig().usePopover&&this._popoverLocation!==`global`,e=this._overlayContainer.getContainerElement();t&&(e.style.display=`block`);let i=e.getBoundingClientRect();return t&&(e.style.display=``),i}};function R$1(o,t){for(let e in t)t.hasOwnProperty(e)&&(o[e]=t[e]);return o}function Lt(o){if(typeof o!=`number`&&o!=null){let[t,e]=o.split(ee$1);return!e||e===`px`?parseFloat(t):null}return o||null}function Xt(o){return{top:Math.floor(o.top),right:Math.floor(o.right),bottom:Math.floor(o.bottom),left:Math.floor(o.left),width:Math.floor(o.width),height:Math.floor(o.height)}}function ie$1(o,t){return o===t?!0:o.isOriginClipped===t.isOriginClipped&&o.isOriginOutsideView===t.isOriginOutsideView&&o.isOverlayClipped===t.isOverlayClipped&&o.isOverlayOutsideView===t.isOverlayOutsideView}var It=`cdk-global-overlay-wrapper`;function $t(o){return new q}var q=class{_overlayRef;_cssPosition=`static`;_topOffset=``;_bottomOffset=``;_alignItems=``;_xPosition=``;_xOffset=``;_width=``;_height=``;_isDisposed=!1;attach(t){let e=t.getConfig();this._overlayRef=t,this._width&&!e.width&&t.updateSize({width:this._width}),this._height&&!e.height&&t.updateSize({height:this._height}),t.hostElement.classList.add(It),this._isDisposed=!1}top(t=``){return this._bottomOffset=``,this._topOffset=t,this._alignItems=`flex-start`,this}left(t=``){return this._xOffset=t,this._xPosition=`left`,this}bottom(t=``){return this._topOffset=``,this._bottomOffset=t,this._alignItems=`flex-end`,this}right(t=``){return this._xOffset=t,this._xPosition=`right`,this}start(t=``){return this._xOffset=t,this._xPosition=`start`,this}end(t=``){return this._xOffset=t,this._xPosition=`end`,this}width(t=``){return this._overlayRef?this._overlayRef.updateSize({width:t}):this._width=t,this}height(t=``){return this._overlayRef?this._overlayRef.updateSize({height:t}):this._height=t,this}centerHorizontally(t=``){return this.left(t),this._xPosition=`center`,this}centerVertically(t=``){return this.top(t),this._alignItems=`center`,this}apply(){if(!this._overlayRef||!this._overlayRef.hasAttached())return;let t=this._overlayRef.overlayElement.style,e=this._overlayRef.hostElement.style,{width:s,height:n,maxWidth:r,maxHeight:a}=this._overlayRef.getConfig(),h=(s===`100%`||s===`100vw`)&&(!r||r===`100%`||r===`100vw`),p=(n===`100%`||n===`100vh`)&&(!a||a===`100%`||a===`100vh`),l=this._xPosition,d=this._xOffset,u=this._overlayRef.getConfig().direction===`rtl`,g=``,_=``,v=``;h?v=`flex-start`:l===`center`?(v=`center`,u?_=d:g=d):u?l===`left`||l===`end`?(v=`flex-end`,g=d):(l===`right`||l===`start`)&&(v=`flex-start`,_=d):l===`left`||l===`start`?(v=`flex-start`,g=d):(l===`right`||l===`end`)&&(v=`flex-end`,_=d),t.position=this._cssPosition,t.marginLeft=h?`0`:g,t.marginTop=p?`0`:this._topOffset,t.marginBottom=this._bottomOffset,t.marginRight=h?`0`:_,e.justifyContent=v,e.alignItems=p?`flex-start`:this._alignItems}dispose(){if(this._isDisposed||!this._overlayRef)return;let t=this._overlayRef.overlayElement.style,e=this._overlayRef.hostElement,i=e.style;e.classList.remove(It),i.justifyContent=i.alignItems=t.marginTop=t.marginBottom=t.marginLeft=t.marginRight=t.position=``,this._overlayRef=null,this._isDisposed=!0}};var qt=(()=>{class o{_injector=T$1(he);global(){return $t()}flexibleConnectedTo(e){return ut(this._injector,e)}static ɵfac=function(i){return new(i||o)};static ɵprov=yr({token:o,factory:o.ɵfac})}return o})();var _t=new A$1(`OVERLAY_DEFAULT_CONFIG`);function gt(o,t){o.get(le$1).load(Gt);let e=o.get(Kt),i=o.get(rr),s=o.get(me$1),n=o.get(Ni$1),r=o.get(Ot$1),a=o.get(La,null,{optional:!0})||o.get(pr).createRenderer(null,null),h=new B(t),p=o.get(_t,null,{optional:!0})?.usePopover??!0;h.direction=h.direction||r.value,!i.body||!(`showPopover`in i.body)?h.usePopover=!1:h.usePopover=t?.usePopover??p;let l=i.createElement(`div`),d=i.createElement(`div`);l.id=s.getId(`cdk-overlay-`),l.classList.add(`cdk-overlay-pane`),d.appendChild(l),h.usePopover&&(d.setAttribute(`popover`,`manual`),d.classList.add(`cdk-overlay-popover`));let u=h.usePopover?h.positionStrategy?.getPopoverInsertionPoint?.():null;return ft(u)?u.after(d):u?.type===`parent`?u.element.appendChild(d):e.getContainerElement().appendChild(d),new K(new j$1(l,n,o),d,l,h,o.get(Se),o.get(Zt),i,o.get(Le),o.get(Ut),t?.disableAnimations??o.get(rm,null,{optional:!0})===`NoopAnimations`,o.get(ce$1),a)}var Jt$1=(()=>{class o{scrollStrategies=T$1(jt$1);_positionBuilder=T$1(qt);_injector=T$1(he);create(e){return gt(this._injector,e)}position(){return this._positionBuilder}static ɵfac=function(i){return new(i||o)};static ɵprov=yr({token:o,factory:o.ɵfac})}return o})();var oe$1=[{originX:`start`,originY:`bottom`,overlayX:`start`,overlayY:`top`},{originX:`start`,originY:`top`,overlayX:`start`,overlayY:`bottom`},{originX:`end`,originY:`top`,overlayX:`end`,overlayY:`bottom`},{originX:`end`,originY:`bottom`,overlayX:`end`,overlayY:`top`}];var se$1=new A$1(`cdk-connected-overlay-scroll-strategy`,{providedIn:`root`,factory:()=>{let o=T$1(he);return()=>pt(o)}});var dt$1=(()=>{class o{elementRef=T$1(vr);static ɵfac=function(i){return new(i||o)};static ɵdir=hE({type:o,selectors:[[``,`cdk-overlay-origin`,``],[``,`overlay-origin`,``],[``,`cdkOverlayOrigin`,``]],exportAs:[`cdkOverlayOrigin`]})}return o})();var Qt$1=new A$1(`cdk-connected-overlay-default-config`);var ne$1=(()=>{class o{_dir=T$1(Ot$1,{optional:!0});_injector=T$1(he);_overlayRef;_templatePortal;_backdropSubscription=F$1.EMPTY;_attachSubscription=F$1.EMPTY;_detachSubscription=F$1.EMPTY;_positionSubscription=F$1.EMPTY;_offsetX;_offsetY;_position;_scrollStrategyFactory=T$1(se$1);_ngZone=T$1(Se);origin;positions;positionStrategy;get offsetX(){return this._offsetX}set offsetX(e){this._offsetX=e,this._position&&this._updatePositionStrategy(this._position)}get offsetY(){return this._offsetY}set offsetY(e){this._offsetY=e,this._position&&this._updatePositionStrategy(this._position)}width;height;minWidth;minHeight;backdropClass;panelClass;viewportMargin=0;scrollStrategy;open=!1;disableClose=!1;transformOriginSelector;hasBackdrop=!1;lockPosition=!1;flexibleDimensions=!1;growAfterOpen=!1;push=!1;disposeOnNavigation=!1;usePopover;matchWidth=!1;set _config(e){typeof e!=`string`&&this._assignConfig(e)}backdropClick=new Be;positionChange=new Be;attach=new Be;detach=new Be;overlayKeydown=new Be;overlayOutsideClick=new Be;constructor(){let e=T$1(fr),i=T$1(Mi$1),s=T$1(Qt$1,{optional:!0}),n=T$1(_t,{optional:!0});this.usePopover=n?.usePopover===!1?null:`global`,this._templatePortal=new M(e,i),this.scrollStrategy=this._scrollStrategyFactory(),s&&this._assignConfig(s)}get overlayRef(){return this._overlayRef}get dir(){return this._dir?this._dir.value:`ltr`}ngOnDestroy(){this._attachSubscription.unsubscribe(),this._detachSubscription.unsubscribe(),this._backdropSubscription.unsubscribe(),this._positionSubscription.unsubscribe(),this._overlayRef?.dispose()}ngOnChanges(e){this._position&&(this._updatePositionStrategy(this._position),this._overlayRef?.updateSize({width:this._getWidth(),minWidth:this.minWidth,height:this.height,minHeight:this.minHeight}),e.origin&&this.open&&this._position.apply()),e.open&&(this.open?this.attachOverlay():this.detachOverlay())}_createOverlay(){(!this.positions||!this.positions.length)&&(this.positions=oe$1);let e=this._overlayRef=gt(this._injector,this._buildConfig());this._attachSubscription=e.attachments().subscribe(()=>this.attach.emit()),this._detachSubscription=e.detachments().subscribe(()=>this.detach.emit()),e.keydownEvents().subscribe(i=>{this.overlayKeydown.next(i),i.keyCode===27&&!this.disableClose&&!st$2(i)&&(i.preventDefault(),this.detachOverlay())}),this._overlayRef.outsidePointerEvents().subscribe(i=>{let s=this._getOriginElement(),n=b$1(i);(!s||s!==n&&!s.contains(n))&&this.overlayOutsideClick.next(i)})}_buildConfig(){let e=this._position=this.positionStrategy||this._createPositionStrategy(),i=new B({direction:this._dir||`ltr`,positionStrategy:e,scrollStrategy:this.scrollStrategy,hasBackdrop:this.hasBackdrop,disposeOnNavigation:this.disposeOnNavigation,usePopover:!!this.usePopover});return(this.height||this.height===0)&&(i.height=this.height),(this.minWidth||this.minWidth===0)&&(i.minWidth=this.minWidth),(this.minHeight||this.minHeight===0)&&(i.minHeight=this.minHeight),this.backdropClass&&(i.backdropClass=this.backdropClass),this.panelClass&&(i.panelClass=this.panelClass),i}_updatePositionStrategy(e){let i=this.positions.map(s=>({originX:s.originX,originY:s.originY,overlayX:s.overlayX,overlayY:s.overlayY,offsetX:s.offsetX||this.offsetX,offsetY:s.offsetY||this.offsetY,panelClass:s.panelClass||void 0}));return e.setOrigin(this._getOrigin()).withPositions(i).withFlexibleDimensions(this.flexibleDimensions).withPush(this.push).withGrowAfterOpen(this.growAfterOpen).withViewportMargin(this.viewportMargin).withLockedPosition(this.lockPosition).withTransformOriginOn(this.transformOriginSelector).withPopoverLocation(this.usePopover===null?`global`:this.usePopover)}_createPositionStrategy(){let e=ut(this._injector,this._getOrigin());return this._updatePositionStrategy(e),e}_getOrigin(){return this.origin instanceof dt$1?this.origin.elementRef:this.origin}_getOriginElement(){return this.origin instanceof dt$1?this.origin.elementRef.nativeElement:this.origin instanceof vr?this.origin.nativeElement:typeof Element<`u`&&this.origin instanceof Element?this.origin:null}_getWidth(){return this.width?this.width:this.matchWidth?this._getOriginElement()?.getBoundingClientRect?.().width:void 0}attachOverlay(){this._overlayRef||this._createOverlay();let e=this._overlayRef;e.getConfig().hasBackdrop=this.hasBackdrop,e.updateSize({width:this._getWidth()}),e.hasAttached()||e.attach(this._templatePortal),this.hasBackdrop?this._backdropSubscription=e.backdropClick().subscribe(i=>this.backdropClick.emit(i)):this._backdropSubscription.unsubscribe(),this._positionSubscription.unsubscribe(),this.positionChange.observers.length>0&&(this._positionSubscription=this._position.positionChanges.pipe(Dg(()=>this.positionChange.observers.length>0)).subscribe(i=>{this._ngZone.run(()=>this.positionChange.emit(i)),this.positionChange.observers.length===0&&this._positionSubscription.unsubscribe()})),this.open=!0}detachOverlay(){this._overlayRef?.detach(),this._backdropSubscription.unsubscribe(),this._positionSubscription.unsubscribe(),this.open=!1}_assignConfig(e){this.origin=e.origin??this.origin,this.positions=e.positions??this.positions,this.positionStrategy=e.positionStrategy??this.positionStrategy,this.offsetX=e.offsetX??this.offsetX,this.offsetY=e.offsetY??this.offsetY,this.width=e.width??this.width,this.height=e.height??this.height,this.minWidth=e.minWidth??this.minWidth,this.minHeight=e.minHeight??this.minHeight,this.backdropClass=e.backdropClass??this.backdropClass,this.panelClass=e.panelClass??this.panelClass,this.viewportMargin=e.viewportMargin??this.viewportMargin,this.scrollStrategy=e.scrollStrategy??this.scrollStrategy,this.disableClose=e.disableClose??this.disableClose,this.transformOriginSelector=e.transformOriginSelector??this.transformOriginSelector,this.hasBackdrop=e.hasBackdrop??this.hasBackdrop,this.lockPosition=e.lockPosition??this.lockPosition,this.flexibleDimensions=e.flexibleDimensions??this.flexibleDimensions,this.growAfterOpen=e.growAfterOpen??this.growAfterOpen,this.push=e.push??this.push,this.disposeOnNavigation=e.disposeOnNavigation??this.disposeOnNavigation,this.usePopover=e.usePopover??this.usePopover,this.matchWidth=e.matchWidth??this.matchWidth}static ɵfac=function(i){return new(i||o)};static ɵdir=hE({type:o,selectors:[[``,`cdk-connected-overlay`,``],[``,`connected-overlay`,``],[``,`cdkConnectedOverlay`,``]],inputs:{origin:[0,`cdkConnectedOverlayOrigin`,`origin`],positions:[0,`cdkConnectedOverlayPositions`,`positions`],positionStrategy:[0,`cdkConnectedOverlayPositionStrategy`,`positionStrategy`],offsetX:[0,`cdkConnectedOverlayOffsetX`,`offsetX`],offsetY:[0,`cdkConnectedOverlayOffsetY`,`offsetY`],width:[0,`cdkConnectedOverlayWidth`,`width`],height:[0,`cdkConnectedOverlayHeight`,`height`],minWidth:[0,`cdkConnectedOverlayMinWidth`,`minWidth`],minHeight:[0,`cdkConnectedOverlayMinHeight`,`minHeight`],backdropClass:[0,`cdkConnectedOverlayBackdropClass`,`backdropClass`],panelClass:[0,`cdkConnectedOverlayPanelClass`,`panelClass`],viewportMargin:[0,`cdkConnectedOverlayViewportMargin`,`viewportMargin`],scrollStrategy:[0,`cdkConnectedOverlayScrollStrategy`,`scrollStrategy`],open:[0,`cdkConnectedOverlayOpen`,`open`],disableClose:[0,`cdkConnectedOverlayDisableClose`,`disableClose`],transformOriginSelector:[0,`cdkConnectedOverlayTransformOriginOn`,`transformOriginSelector`],hasBackdrop:[2,`cdkConnectedOverlayHasBackdrop`,`hasBackdrop`,$F],lockPosition:[2,`cdkConnectedOverlayLockPosition`,`lockPosition`,$F],flexibleDimensions:[2,`cdkConnectedOverlayFlexibleDimensions`,`flexibleDimensions`,$F],growAfterOpen:[2,`cdkConnectedOverlayGrowAfterOpen`,`growAfterOpen`,$F],push:[2,`cdkConnectedOverlayPush`,`push`,$F],disposeOnNavigation:[2,`cdkConnectedOverlayDisposeOnNavigation`,`disposeOnNavigation`,$F],usePopover:[0,`cdkConnectedOverlayUsePopover`,`usePopover`],matchWidth:[2,`cdkConnectedOverlayMatchWidth`,`matchWidth`,$F],_config:[0,`cdkConnectedOverlay`,`_config`]},outputs:{backdropClick:`backdropClick`,positionChange:`positionChange`,attach:`attach`,detach:`detach`,overlayKeydown:`overlayKeydown`,overlayOutsideClick:`overlayOutsideClick`},exportAs:[`cdkConnectedOverlay`],features:[Cm]})}return o})();var re$1=(()=>{class o{static ɵfac=function(i){return new(i||o)};static ɵmod=dE({type:o});static ɵinj=Gl({providers:[Jt$1],imports:[at$2,Vt,Ze,Ze]})}return o})();var O=new A$1(`MAT_BUTTON_CONFIG`);function R(n){return n==null?void 0:UF(n)}var j=(()=>{class n{_elementRef=T$1(vr);_ngZone=T$1(Se);_animationsDisabled=J$2();_config=T$1(O,{optional:!0});_focusMonitor=T$1(_t$1);_cleanupClick;_renderer=T$1(La);_rippleLoader=T$1(Es);_isAnchor;_isFab=!1;color;get disableRipple(){return this._disableRipple}set disableRipple(t){this._disableRipple=t,this._updateRippleDisabled()}_disableRipple=!1;get disabled(){return this._disabled}set disabled(t){this._disabled=t,this._updateRippleDisabled()}_disabled=!1;ariaDisabled;disabledInteractive;tabIndex;set _tabindex(t){this.tabIndex=t}showProgress=PF(!1,{transform:$F});constructor(){T$1(le$1).load(Is);let t=this._elementRef.nativeElement;this._isAnchor=t.tagName===`A`,this.disabledInteractive=this._config?.disabledInteractive??!1,this.color=this._config?.color??null,this._rippleLoader?.configureRipple(t,{className:`mat-mdc-button-ripple`})}ngAfterViewInit(){this._focusMonitor.monitor(this._elementRef,!0),this._isAnchor&&this._setupAsAnchor()}ngOnDestroy(){this._cleanupClick?.(),this._focusMonitor.stopMonitoring(this._elementRef),this._rippleLoader?.destroyRipple(this._elementRef.nativeElement)}focus(t=`program`,e){t?this._focusMonitor.focusVia(this._elementRef.nativeElement,t,e):this._elementRef.nativeElement.focus(e)}_getAriaDisabled(){return this.ariaDisabled!=null?this.ariaDisabled:this._isAnchor?this.disabled||null:this.disabled&&this.disabledInteractive?!0:null}_getDisabledAttribute(){return this.disabledInteractive||!this.disabled?null:!0}_updateRippleDisabled(){this._rippleLoader?.setDisabled(this._elementRef.nativeElement,this.disableRipple||this.disabled)}_getTabIndex(){return this._isAnchor?this.disabled&&!this.disabledInteractive?-1:this.tabIndex:this.tabIndex}_setupAsAnchor(){this._cleanupClick=this._ngZone.runOutsideAngular(()=>this._renderer.listen(this._elementRef.nativeElement,`click`,t=>{this.disabled&&(t.preventDefault(),t.stopImmediatePropagation())}))}static ɵfac=function(e){return new(e||n)};static ɵdir=hE({type:n,hostAttrs:[1,`mat-mdc-button-base`],hostVars:15,hostBindings:function(e,a){e&2&&(_p(`disabled`,a._getDisabledAttribute())(`aria-disabled`,a._getAriaDisabled())(`tabindex`,a._getTabIndex()),cD(a.color?`mat-`+a.color:``),Bp(`mat-mdc-button-progress-indicator-shown`,a.showProgress())(`mat-mdc-button-disabled`,a.disabled)(`mat-mdc-button-disabled-interactive`,a.disabledInteractive)(`mat-unthemed`,!a.color)(`_mat-animation-noopable`,a._animationsDisabled))},inputs:{color:`color`,disableRipple:[2,`disableRipple`,`disableRipple`,$F],disabled:[2,`disabled`,`disabled`,$F],ariaDisabled:[2,`aria-disabled`,`ariaDisabled`,$F],disabledInteractive:[2,`disabledInteractive`,`disabledInteractive`,$F],tabIndex:[2,`tabIndex`,`tabIndex`,R],_tabindex:[2,`tabindex`,`_tabindex`,R],showProgress:[1,`showProgress`]}})}return n})();var V=[[[``,8,`material-icons`,3,`iconPositionEnd`,``],[`mat-icon`,3,`iconPositionEnd`,``],[``,`matButtonIcon`,``,3,`iconPositionEnd`,``]],`*`,[[``,`iconPositionEnd`,``,8,`material-icons`],[`mat-icon`,`iconPositionEnd`,``],[``,`matButtonIcon`,``,`iconPositionEnd`,``]],[[``,`progressIndicator`,``]]];var U=[`.material-icons:not([iconPositionEnd]), mat-icon:not([iconPositionEnd]), [matButtonIcon]:not([iconPositionEnd])`,`*`,`.material-icons[iconPositionEnd], mat-icon[iconPositionEnd], [matButtonIcon][iconPositionEnd]`,`[progressIndicator]`];function Y(n,Z){n&1&&(Rc(0,`div`,2),GE(1,3),kc())}var P=new Map([[`text`,[`mat-mdc-button`]],[`filled`,[`mdc-button--unelevated`,`mat-mdc-unelevated-button`]],[`elevated`,[`mdc-button--raised`,`mat-mdc-raised-button`]],[`outlined`,[`mdc-button--outlined`,`mat-mdc-outlined-button`]],[`tonal`,[`mat-tonal-button`]]]);var mt$1=(()=>{class n extends j{get appearance(){return this._appearance}set appearance(t){this.setAppearance(t||this._config?.defaultAppearance||`text`)}_appearance=null;constructor(){super();let t=G(this._elementRef.nativeElement);t&&this.setAppearance(t)}setAppearance(t){if(t===this._appearance)return;let e=this._elementRef.nativeElement.classList,a=this._appearance?P.get(this._appearance):null,T=P.get(t);a&&e.remove(...a),e.add(...T),this._appearance=t}static ɵfac=function(e){return new(e||n)};static ɵcmp=lE({type:n,selectors:[[`button`,`matButton`,``],[`a`,`matButton`,``],[`button`,`mat-button`,``],[`button`,`mat-raised-button`,``],[`button`,`mat-flat-button`,``],[`button`,`mat-stroked-button`,``],[`a`,`mat-button`,``],[`a`,`mat-raised-button`,``],[`a`,`mat-flat-button`,``],[`a`,`mat-stroked-button`,``]],hostAttrs:[1,`mdc-button`],inputs:{appearance:[0,`matButton`,`appearance`]},exportAs:[`matButton`,`matAnchor`],features:[Ep],ngContentSelectors:U,decls:8,vars:5,consts:[[1,`mat-mdc-button-persistent-ripple`],[1,`mdc-button__label`],[1,`mat-mdc-button-progress-indicator-container`],[1,`mat-focus-indicator`],[1,`mat-mdc-button-touch-target`]],template:function(e,a){e&1&&(WE(V),Sp(0,`span`,0),GE(1),Rc(2,`span`,1),GE(3,1),kc(),GE(4,2),xE(5,Y,2,0,`div`,2),Sp(6,`span`,3)(7,`span`,4)),e&2&&(Bp(`mdc-button__ripple`,!a._isFab)(`mdc-fab__ripple`,a._isFab),Sv(5),AE(a.showProgress()?5:-1))},styles:[`.mat-mdc-button-base {
  text-decoration: none;
}
.mat-mdc-button-base .mat-icon {
  min-height: fit-content;
  flex-shrink: 0;
}
@media (hover: none) {
  .mat-mdc-button-base:hover > span.mat-mdc-button-persistent-ripple::before {
    opacity: 0;
  }
}

.mdc-button {
  -webkit-user-select: none;
  user-select: none;
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
  min-width: 64px;
  border: none;
  outline: none;
  line-height: inherit;
  -webkit-appearance: none;
  overflow: visible;
  vertical-align: middle;
  background: transparent;
  padding: 0 8px;
}
.mdc-button::-moz-focus-inner {
  padding: 0;
  border: 0;
}
.mdc-button:active {
  outline: none;
}
.mdc-button:hover {
  cursor: pointer;
}
.mdc-button:disabled {
  cursor: default;
  pointer-events: none;
}
.mdc-button[hidden] {
  display: none;
}
.mdc-button .mdc-button__label {
  position: relative;
}

.mat-mdc-button {
  padding: 0 var(--%NS%mat-button-text-horizontal-padding, 12px);
  height: var(--%NS%mat-button-text-container-height, 40px);
  font-family: var(--%NS%mat-button-text-label-text-font, var(--%NS%mat-sys-label-large-font));
  font-size: var(--%NS%mat-button-text-label-text-size, var(--%NS%mat-sys-label-large-size));
  letter-spacing: var(--%NS%mat-button-text-label-text-tracking, var(--%NS%mat-sys-label-large-tracking));
  text-transform: var(--%NS%mat-button-text-label-text-transform);
  font-weight: var(--%NS%mat-button-text-label-text-weight, var(--%NS%mat-sys-label-large-weight));
}
.mat-mdc-button, .mat-mdc-button .mdc-button__ripple {
  border-radius: var(--%NS%mat-button-text-container-shape, var(--%NS%mat-sys-corner-full));
}
.mat-mdc-button:not(:disabled) {
  color: var(--%NS%mat-button-text-label-text-color, var(--%NS%mat-sys-primary));
}
.mat-mdc-button[disabled], .mat-mdc-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--%NS%mat-button-text-disabled-label-text-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
}
.mat-mdc-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}
.mat-mdc-button:has(.material-icons, mat-icon, [matButtonIcon]) {
  padding: 0 var(--%NS%mat-button-text-with-icon-horizontal-padding, 16px);
}
.mat-mdc-button > .mat-icon {
  margin-right: var(--%NS%mat-button-text-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-text-icon-offset, -4px);
}
[dir=rtl] .mat-mdc-button > .mat-icon {
  margin-right: var(--%NS%mat-button-text-icon-offset, -4px);
  margin-left: var(--%NS%mat-button-text-icon-spacing, 8px);
}
.mat-mdc-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-text-icon-offset, -4px);
  margin-left: var(--%NS%mat-button-text-icon-spacing, 8px);
}
[dir=rtl] .mat-mdc-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-text-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-text-icon-offset, -4px);
}
.mat-mdc-button .mat-ripple-element {
  background-color: var(--%NS%mat-button-text-ripple-color, color-mix(in srgb, var(--%NS%mat-sys-primary) calc(var(--%NS%mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-text-state-layer-color, var(--%NS%mat-sys-primary));
}
.mat-mdc-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-text-disabled-state-layer-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-mdc-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-text-hover-state-layer-opacity, var(--%NS%mat-sys-hover-state-layer-opacity));
}
.mat-mdc-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-text-focus-state-layer-opacity, var(--%NS%mat-sys-focus-state-layer-opacity));
}
.mat-mdc-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-text-pressed-state-layer-opacity, var(--%NS%mat-sys-pressed-state-layer-opacity));
}
.mat-mdc-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--%NS%mat-button-text-touch-target-size, 48px);
  display: var(--%NS%mat-button-text-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}

.mat-mdc-unelevated-button {
  transition: box-shadow 280ms cubic-bezier(0.4, 0, 0.2, 1);
  height: var(--%NS%mat-button-filled-container-height, 40px);
  font-family: var(--%NS%mat-button-filled-label-text-font, var(--%NS%mat-sys-label-large-font));
  font-size: var(--%NS%mat-button-filled-label-text-size, var(--%NS%mat-sys-label-large-size));
  letter-spacing: var(--%NS%mat-button-filled-label-text-tracking, var(--%NS%mat-sys-label-large-tracking));
  text-transform: var(--%NS%mat-button-filled-label-text-transform);
  font-weight: var(--%NS%mat-button-filled-label-text-weight, var(--%NS%mat-sys-label-large-weight));
  padding: 0 var(--%NS%mat-button-filled-horizontal-padding, 24px);
}
.mat-mdc-unelevated-button > .mat-icon {
  margin-right: var(--%NS%mat-button-filled-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-filled-icon-offset, -8px);
}
[dir=rtl] .mat-mdc-unelevated-button > .mat-icon {
  margin-right: var(--%NS%mat-button-filled-icon-offset, -8px);
  margin-left: var(--%NS%mat-button-filled-icon-spacing, 8px);
}
.mat-mdc-unelevated-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-filled-icon-offset, -8px);
  margin-left: var(--%NS%mat-button-filled-icon-spacing, 8px);
}
[dir=rtl] .mat-mdc-unelevated-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-filled-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-filled-icon-offset, -8px);
}
.mat-mdc-unelevated-button .mat-ripple-element {
  background-color: var(--%NS%mat-button-filled-ripple-color, color-mix(in srgb, var(--%NS%mat-sys-on-primary) calc(var(--%NS%mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-unelevated-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-filled-state-layer-color, var(--%NS%mat-sys-on-primary));
}
.mat-mdc-unelevated-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-filled-disabled-state-layer-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-mdc-unelevated-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-filled-hover-state-layer-opacity, var(--%NS%mat-sys-hover-state-layer-opacity));
}
.mat-mdc-unelevated-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-unelevated-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-unelevated-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-filled-focus-state-layer-opacity, var(--%NS%mat-sys-focus-state-layer-opacity));
}
.mat-mdc-unelevated-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-filled-pressed-state-layer-opacity, var(--%NS%mat-sys-pressed-state-layer-opacity));
}
.mat-mdc-unelevated-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--%NS%mat-button-filled-touch-target-size, 48px);
  display: var(--%NS%mat-button-filled-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}
.mat-mdc-unelevated-button:not(:disabled) {
  color: var(--%NS%mat-button-filled-label-text-color, var(--%NS%mat-sys-on-primary));
  background-color: var(--%NS%mat-button-filled-container-color, var(--%NS%mat-sys-primary));
}
.mat-mdc-unelevated-button, .mat-mdc-unelevated-button .mdc-button__ripple {
  border-radius: var(--%NS%mat-button-filled-container-shape, var(--%NS%mat-sys-corner-full));
}
.mat-mdc-unelevated-button .mat-mdc-button-progress-indicator-container {
  --%NS%mat-progress-spinner-active-indicator-color: var(--%NS%mat-button-filled-progress-active-indicator-color, var(--%NS%mat-sys-on-primary));
}
.mat-mdc-unelevated-button[disabled], .mat-mdc-unelevated-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--%NS%mat-button-filled-disabled-label-text-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
  background-color: var(--%NS%mat-button-filled-disabled-container-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 12%, transparent));
}
.mat-mdc-unelevated-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}

.mat-mdc-raised-button {
  transition: box-shadow 280ms cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: var(--%NS%mat-button-protected-container-elevation-shadow, var(--%NS%mat-sys-level1));
  height: var(--%NS%mat-button-protected-container-height, 40px);
  font-family: var(--%NS%mat-button-protected-label-text-font, var(--%NS%mat-sys-label-large-font));
  font-size: var(--%NS%mat-button-protected-label-text-size, var(--%NS%mat-sys-label-large-size));
  letter-spacing: var(--%NS%mat-button-protected-label-text-tracking, var(--%NS%mat-sys-label-large-tracking));
  text-transform: var(--%NS%mat-button-protected-label-text-transform);
  font-weight: var(--%NS%mat-button-protected-label-text-weight, var(--%NS%mat-sys-label-large-weight));
  padding: 0 var(--%NS%mat-button-protected-horizontal-padding, 24px);
}
.mat-mdc-raised-button > .mat-icon {
  margin-right: var(--%NS%mat-button-protected-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-protected-icon-offset, -8px);
}
[dir=rtl] .mat-mdc-raised-button > .mat-icon {
  margin-right: var(--%NS%mat-button-protected-icon-offset, -8px);
  margin-left: var(--%NS%mat-button-protected-icon-spacing, 8px);
}
.mat-mdc-raised-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-protected-icon-offset, -8px);
  margin-left: var(--%NS%mat-button-protected-icon-spacing, 8px);
}
[dir=rtl] .mat-mdc-raised-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-protected-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-protected-icon-offset, -8px);
}
.mat-mdc-raised-button .mat-ripple-element {
  background-color: var(--%NS%mat-button-protected-ripple-color, color-mix(in srgb, var(--%NS%mat-sys-primary) calc(var(--%NS%mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-raised-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-protected-state-layer-color, var(--%NS%mat-sys-primary));
}
.mat-mdc-raised-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-protected-disabled-state-layer-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-mdc-raised-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-protected-hover-state-layer-opacity, var(--%NS%mat-sys-hover-state-layer-opacity));
}
.mat-mdc-raised-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-raised-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-raised-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-protected-focus-state-layer-opacity, var(--%NS%mat-sys-focus-state-layer-opacity));
}
.mat-mdc-raised-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-protected-pressed-state-layer-opacity, var(--%NS%mat-sys-pressed-state-layer-opacity));
}
.mat-mdc-raised-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--%NS%mat-button-protected-touch-target-size, 48px);
  display: var(--%NS%mat-button-protected-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}
.mat-mdc-raised-button:not(:disabled) {
  color: var(--%NS%mat-button-protected-label-text-color, var(--%NS%mat-sys-primary));
  background-color: var(--%NS%mat-button-protected-container-color, var(--%NS%mat-sys-surface));
}
.mat-mdc-raised-button, .mat-mdc-raised-button .mdc-button__ripple {
  border-radius: var(--%NS%mat-button-protected-container-shape, var(--%NS%mat-sys-corner-full));
}
@media (hover: hover) {
  .mat-mdc-raised-button:hover {
    box-shadow: var(--%NS%mat-button-protected-hover-container-elevation-shadow, var(--%NS%mat-sys-level2));
  }
}
.mat-mdc-raised-button:focus {
  box-shadow: var(--%NS%mat-button-protected-focus-container-elevation-shadow, var(--%NS%mat-sys-level1));
}
.mat-mdc-raised-button:active, .mat-mdc-raised-button:focus:active {
  box-shadow: var(--%NS%mat-button-protected-pressed-container-elevation-shadow, var(--%NS%mat-sys-level1));
}
.mat-mdc-raised-button[disabled], .mat-mdc-raised-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--%NS%mat-button-protected-disabled-label-text-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
  background-color: var(--%NS%mat-button-protected-disabled-container-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 12%, transparent));
}
.mat-mdc-raised-button[disabled].mat-mdc-button-disabled, .mat-mdc-raised-button.mat-mdc-button-disabled.mat-mdc-button-disabled {
  box-shadow: var(--%NS%mat-button-protected-disabled-container-elevation-shadow, var(--%NS%mat-sys-level0));
}
.mat-mdc-raised-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}

.mat-mdc-outlined-button {
  border-style: solid;
  transition: border 280ms cubic-bezier(0.4, 0, 0.2, 1);
  height: var(--%NS%mat-button-outlined-container-height, 40px);
  font-family: var(--%NS%mat-button-outlined-label-text-font, var(--%NS%mat-sys-label-large-font));
  font-size: var(--%NS%mat-button-outlined-label-text-size, var(--%NS%mat-sys-label-large-size));
  letter-spacing: var(--%NS%mat-button-outlined-label-text-tracking, var(--%NS%mat-sys-label-large-tracking));
  text-transform: var(--%NS%mat-button-outlined-label-text-transform);
  font-weight: var(--%NS%mat-button-outlined-label-text-weight, var(--%NS%mat-sys-label-large-weight));
  border-radius: var(--%NS%mat-button-outlined-container-shape, var(--%NS%mat-sys-corner-full));
  border-width: var(--%NS%mat-button-outlined-outline-width, 1px);
  padding: 0 var(--%NS%mat-button-outlined-horizontal-padding, 24px);
}
.mat-mdc-outlined-button > .mat-icon {
  margin-right: var(--%NS%mat-button-outlined-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-outlined-icon-offset, -8px);
}
[dir=rtl] .mat-mdc-outlined-button > .mat-icon {
  margin-right: var(--%NS%mat-button-outlined-icon-offset, -8px);
  margin-left: var(--%NS%mat-button-outlined-icon-spacing, 8px);
}
.mat-mdc-outlined-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-outlined-icon-offset, -8px);
  margin-left: var(--%NS%mat-button-outlined-icon-spacing, 8px);
}
[dir=rtl] .mat-mdc-outlined-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-outlined-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-outlined-icon-offset, -8px);
}
.mat-mdc-outlined-button .mat-ripple-element {
  background-color: var(--%NS%mat-button-outlined-ripple-color, color-mix(in srgb, var(--%NS%mat-sys-primary) calc(var(--%NS%mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-outlined-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-outlined-state-layer-color, var(--%NS%mat-sys-primary));
}
.mat-mdc-outlined-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-outlined-disabled-state-layer-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-mdc-outlined-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-outlined-hover-state-layer-opacity, var(--%NS%mat-sys-hover-state-layer-opacity));
}
.mat-mdc-outlined-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-outlined-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-outlined-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-outlined-focus-state-layer-opacity, var(--%NS%mat-sys-focus-state-layer-opacity));
}
.mat-mdc-outlined-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-outlined-pressed-state-layer-opacity, var(--%NS%mat-sys-pressed-state-layer-opacity));
}
.mat-mdc-outlined-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--%NS%mat-button-outlined-touch-target-size, 48px);
  display: var(--%NS%mat-button-outlined-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}
.mat-mdc-outlined-button:not(:disabled) {
  color: var(--%NS%mat-button-outlined-label-text-color, var(--%NS%mat-sys-primary));
  border-color: var(--%NS%mat-button-outlined-outline-color, var(--%NS%mat-sys-outline));
}
.mat-mdc-outlined-button[disabled], .mat-mdc-outlined-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--%NS%mat-button-outlined-disabled-label-text-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
  border-color: var(--%NS%mat-button-outlined-disabled-outline-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 12%, transparent));
}
.mat-mdc-outlined-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}

.mat-tonal-button {
  transition: box-shadow 280ms cubic-bezier(0.4, 0, 0.2, 1);
  height: var(--%NS%mat-button-tonal-container-height, 40px);
  font-family: var(--%NS%mat-button-tonal-label-text-font, var(--%NS%mat-sys-label-large-font));
  font-size: var(--%NS%mat-button-tonal-label-text-size, var(--%NS%mat-sys-label-large-size));
  letter-spacing: var(--%NS%mat-button-tonal-label-text-tracking, var(--%NS%mat-sys-label-large-tracking));
  text-transform: var(--%NS%mat-button-tonal-label-text-transform);
  font-weight: var(--%NS%mat-button-tonal-label-text-weight, var(--%NS%mat-sys-label-large-weight));
  padding: 0 var(--%NS%mat-button-tonal-horizontal-padding, 24px);
}
.mat-tonal-button:not(:disabled) {
  color: var(--%NS%mat-button-tonal-label-text-color, var(--%NS%mat-sys-on-secondary-container));
  background-color: var(--%NS%mat-button-tonal-container-color, var(--%NS%mat-sys-secondary-container));
}
.mat-tonal-button, .mat-tonal-button .mdc-button__ripple {
  border-radius: var(--%NS%mat-button-tonal-container-shape, var(--%NS%mat-sys-corner-full));
}
.mat-tonal-button[disabled], .mat-tonal-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--%NS%mat-button-tonal-disabled-label-text-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 38%, transparent));
  background-color: var(--%NS%mat-button-tonal-disabled-container-color, color-mix(in srgb, var(--%NS%mat-sys-on-surface) 12%, transparent));
}
.mat-tonal-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}
.mat-tonal-button > .mat-icon {
  margin-right: var(--%NS%mat-button-tonal-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-tonal-icon-offset, -8px);
}
[dir=rtl] .mat-tonal-button > .mat-icon {
  margin-right: var(--%NS%mat-button-tonal-icon-offset, -8px);
  margin-left: var(--%NS%mat-button-tonal-icon-spacing, 8px);
}
.mat-tonal-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-tonal-icon-offset, -8px);
  margin-left: var(--%NS%mat-button-tonal-icon-spacing, 8px);
}
[dir=rtl] .mat-tonal-button .mdc-button__label + .mat-icon {
  margin-right: var(--%NS%mat-button-tonal-icon-spacing, 8px);
  margin-left: var(--%NS%mat-button-tonal-icon-offset, -8px);
}
.mat-tonal-button .mat-ripple-element {
  background-color: var(--%NS%mat-button-tonal-ripple-color, color-mix(in srgb, var(--%NS%mat-sys-on-secondary-container) calc(var(--%NS%mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-tonal-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-tonal-state-layer-color, var(--%NS%mat-sys-on-secondary-container));
}
.mat-tonal-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--%NS%mat-button-tonal-disabled-state-layer-color, var(--%NS%mat-sys-on-surface-variant));
}
.mat-tonal-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-tonal-hover-state-layer-opacity, var(--%NS%mat-sys-hover-state-layer-opacity));
}
.mat-tonal-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-tonal-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-tonal-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-tonal-focus-state-layer-opacity, var(--%NS%mat-sys-focus-state-layer-opacity));
}
.mat-tonal-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--%NS%mat-button-tonal-pressed-state-layer-opacity, var(--%NS%mat-sys-pressed-state-layer-opacity));
}
.mat-tonal-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--%NS%mat-button-tonal-touch-target-size, 48px);
  display: var(--%NS%mat-button-tonal-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}

.mat-mdc-button,
.mat-mdc-unelevated-button,
.mat-mdc-raised-button,
.mat-mdc-outlined-button,
.mat-tonal-button {
  -webkit-tap-highlight-color: transparent;
}
.mat-mdc-button .mat-mdc-button-ripple,
.mat-mdc-button .mat-mdc-button-persistent-ripple,
.mat-mdc-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-unelevated-button .mat-mdc-button-ripple,
.mat-mdc-unelevated-button .mat-mdc-button-persistent-ripple,
.mat-mdc-unelevated-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-raised-button .mat-mdc-button-ripple,
.mat-mdc-raised-button .mat-mdc-button-persistent-ripple,
.mat-mdc-raised-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-outlined-button .mat-mdc-button-ripple,
.mat-mdc-outlined-button .mat-mdc-button-persistent-ripple,
.mat-mdc-outlined-button .mat-mdc-button-persistent-ripple::before,
.mat-tonal-button .mat-mdc-button-ripple,
.mat-tonal-button .mat-mdc-button-persistent-ripple,
.mat-tonal-button .mat-mdc-button-persistent-ripple::before {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  pointer-events: none;
  border-radius: inherit;
}
.mat-mdc-button .mat-mdc-button-ripple,
.mat-mdc-unelevated-button .mat-mdc-button-ripple,
.mat-mdc-raised-button .mat-mdc-button-ripple,
.mat-mdc-outlined-button .mat-mdc-button-ripple,
.mat-tonal-button .mat-mdc-button-ripple {
  overflow: hidden;
}
.mat-mdc-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-unelevated-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-raised-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-outlined-button .mat-mdc-button-persistent-ripple::before,
.mat-tonal-button .mat-mdc-button-persistent-ripple::before {
  content: "";
  opacity: 0;
}
.mat-mdc-button .mdc-button__label,
.mat-mdc-button .mat-icon,
.mat-mdc-unelevated-button .mdc-button__label,
.mat-mdc-unelevated-button .mat-icon,
.mat-mdc-raised-button .mdc-button__label,
.mat-mdc-raised-button .mat-icon,
.mat-mdc-outlined-button .mdc-button__label,
.mat-mdc-outlined-button .mat-icon,
.mat-tonal-button .mdc-button__label,
.mat-tonal-button .mat-icon {
  z-index: 1;
  position: relative;
}
.mat-mdc-button .mat-focus-indicator,
.mat-mdc-unelevated-button .mat-focus-indicator,
.mat-mdc-raised-button .mat-focus-indicator,
.mat-mdc-outlined-button .mat-focus-indicator,
.mat-tonal-button .mat-focus-indicator {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  border-radius: inherit;
}
.mat-mdc-button:focus-visible > .mat-focus-indicator::before,
.mat-mdc-unelevated-button:focus-visible > .mat-focus-indicator::before,
.mat-mdc-raised-button:focus-visible > .mat-focus-indicator::before,
.mat-mdc-outlined-button:focus-visible > .mat-focus-indicator::before,
.mat-tonal-button:focus-visible > .mat-focus-indicator::before {
  content: "";
  border-radius: inherit;
}
.mat-mdc-button._mat-animation-noopable,
.mat-mdc-unelevated-button._mat-animation-noopable,
.mat-mdc-raised-button._mat-animation-noopable,
.mat-mdc-outlined-button._mat-animation-noopable,
.mat-tonal-button._mat-animation-noopable {
  transition: none !important;
  animation: none !important;
}
.mat-mdc-button > .mat-icon,
.mat-mdc-unelevated-button > .mat-icon,
.mat-mdc-raised-button > .mat-icon,
.mat-mdc-outlined-button > .mat-icon,
.mat-tonal-button > .mat-icon {
  display: inline-block;
  position: relative;
  vertical-align: top;
  font-size: 1.125rem;
  height: 1.125rem;
  width: 1.125rem;
}

.mat-mdc-outlined-button .mat-mdc-button-ripple,
.mat-mdc-outlined-button .mdc-button__ripple {
  top: -1px;
  left: -1px;
  bottom: -1px;
  right: -1px;
}

.mat-mdc-unelevated-button .mat-focus-indicator::before,
.mat-tonal-button .mat-focus-indicator::before,
.mat-mdc-raised-button .mat-focus-indicator::before {
  margin: calc(calc(var(--%NS%mat-focus-indicator-border-width, 3px) + 2px) * -1);
}

.mat-mdc-outlined-button .mat-focus-indicator::before {
  margin: calc(calc(var(--%NS%mat-focus-indicator-border-width, 3px) + 3px) * -1);
}

.mat-mdc-button-progress-indicator-container {
  position: absolute;
  inset-inline-start: 0;
  inset-block-start: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  box-sizing: border-box;
}

.mat-mdc-button-progress-indicator-shown mat-icon,
.mat-mdc-button-progress-indicator-shown [matButtonIcon],
.mat-mdc-button-progress-indicator-shown .mdc-button__label {
  visibility: hidden;
}
`,`@media (forced-colors: active) {
  .mat-mdc-button:not(.mdc-button--outlined),
  .mat-mdc-unelevated-button:not(.mdc-button--outlined),
  .mat-mdc-raised-button:not(.mdc-button--outlined),
  .mat-mdc-outlined-button:not(.mdc-button--outlined),
  .mat-mdc-button-base.mat-tonal-button,
  .mat-mdc-icon-button.mat-mdc-icon-button,
  .mat-mdc-outlined-button .mdc-button__ripple {
    outline: solid 1px;
  }
}
`],encapsulation:2})}return n})();function G(n){return n.hasAttribute(`mat-raised-button`)?`elevated`:n.hasAttribute(`mat-stroked-button`)?`outlined`:n.hasAttribute(`mat-flat-button`)?`filled`:n.hasAttribute(`mat-button`)?`text`:null}var st=(()=>{class n{static ɵfac=function(e){return new(e||n)};static ɵmod=dE({type:n});static ɵinj=Gl({imports:[Ds,at$2]})}return n})();var o={accessToken:null,expiresAt:null,user:null,isLoading:!1,error:null};var f=mn({name:`auth`,reducer:bn(o,Rn$1(In.loginSubmitted,r=>W$2(q$2({},r),{isLoading:!0,error:null})),Rn$1(In.loginSuccess,In.tokenRefreshed,(r,{accessToken:l,expiresAt:u,user:c})=>W$2(q$2({},r),{isLoading:!1,accessToken:l,expiresAt:u,user:c,error:null})),Rn$1(In.loginFailure,(r,{error:l})=>W$2(q$2({},r),{isLoading:!1,error:l})),Rn$1(In.sessionCleared,In.logoutCompleted,()=>o))});var p=()=>{let c=T$1(X$1),n=T$1(Ke);return c.selectSignal(f.selectAccessToken)()?!0:n.createUrlTree([`/auth/login`])};var ui=`@`;var fi=(()=>{class e{doc;delegate;zone;animationType;moduleImpl;_rendererFactoryPromise=null;scheduler=null;injector=T$1(he);loadingSchedulerFn=T$1(hi,{optional:!0});_engine;constructor(t,n,i,r,a){this.doc=t,this.delegate=n,this.zone=i,this.animationType=r,this.moduleImpl=a}ngOnDestroy(){this._engine?.flush()}loadImpl(){let t=()=>this.moduleImpl??import(`./chunk-CX-nhXqr.js`).then(i=>i),n;return this.loadingSchedulerFn?n=this.loadingSchedulerFn(t):n=t(),n.catch(i=>{throw new M$1(5300,!1)}).then(({ɵcreateEngine:i,ɵAnimationRendererFactory:r})=>{this._engine=i(this.animationType,this.doc);let a=new r(this.delegate,this._engine,this.zone);return this.delegate=a,a})}createRenderer(t,n){let i=this.delegate.createRenderer(t,n);if(i.ɵtype===0)return i;typeof i.throwOnSyntheticProps==`boolean`&&(i.throwOnSyntheticProps=!1);let r=new Qt(i);return n?.data?.animation&&!this._rendererFactoryPromise&&(this._rendererFactoryPromise=this.loadImpl()),this._rendererFactoryPromise?.then(a=>{let d=a.createRenderer(t,n);r.use(d),this.scheduler??=this.injector.get(xe,null,{optional:!0}),this.scheduler?.notify(10)}).catch(a=>{r.use(i)}),r}begin(){this.delegate.begin?.()}end(){this.delegate.end?.()}whenRenderingDone(){return this.delegate.whenRenderingDone?.()??Promise.resolve()}componentReplaced(t){this._engine?.flush(),this.delegate.componentReplaced?.(t)}static ɵfac=function(n){MI()};static ɵprov=oe$2({token:e,factory:e.ɵfac})}return e})();var Qt=class{delegate;replay=[];ɵtype=1;constructor(o){this.delegate=o}use(o){if(this.delegate=o,this.replay!==null){for(let t of this.replay)t(o);this.replay=null}}get data(){return this.delegate.data}destroy(){this.replay=null,this.delegate.destroy()}createElement(o,t){return this.delegate.createElement(o,t)}createComment(o){return this.delegate.createComment(o)}createText(o){return this.delegate.createText(o)}get destroyNode(){return this.delegate.destroyNode}appendChild(o,t){this.delegate.appendChild(o,t)}insertBefore(o,t,n,i){this.delegate.insertBefore(o,t,n,i)}removeChild(o,t,n,i){this.delegate.removeChild(o,t,n,i)}selectRootElement(o,t){return this.delegate.selectRootElement(o,t)}parentNode(o){return this.delegate.parentNode(o)}nextSibling(o){return this.delegate.nextSibling(o)}setAttribute(o,t,n,i){this.delegate.setAttribute(o,t,n,i)}removeAttribute(o,t,n){this.delegate.removeAttribute(o,t,n)}addClass(o,t){this.delegate.addClass(o,t)}removeClass(o,t){this.delegate.removeClass(o,t)}setStyle(o,t,n,i){this.delegate.setStyle(o,t,n,i)}removeStyle(o,t,n){this.delegate.removeStyle(o,t,n)}setProperty(o,t,n){this.shouldReplay(t)&&this.replay.push(i=>i.setProperty(o,t,n)),this.delegate.setProperty(o,t,n)}setValue(o,t){this.delegate.setValue(o,t)}listen(o,t,n,i){return this.shouldReplay(t)&&this.replay.push(r=>r.listen(o,t,n,i)),this.delegate.listen(o,t,n,i)}shouldReplay(o){return this.replay!==null&&o.startsWith(ui)}};var hi=new A$1(``);function Nn(e=`animations`){return dt$3(`NgAsyncAnimations`),To([{provide:pr,useFactory:()=>new fi(T$1(rr),T$1(er),T$1(Se),e)},{provide:rm,useValue:e===`noop`?`NoopAnimations`:`BrowserAnimations`}])}var dt=`PERFORM_ACTION`;var gi=`REFRESH`;var zn=`RESET`;var Ln=`ROLLBACK`;var Hn=`COMMIT`;var $n=`SWEEP`;var Un=`TOGGLE_ACTION`;var bi=`SET_ACTIONS_ACTIVE`;var Vn=`JUMP_TO_STATE`;var qn=`JUMP_TO_ACTION`;var le=`IMPORT_STATE`;var Gn=`LOCK_CHANGES`;var Zn=`PAUSE_RECORDING`;var tt=class{constructor(o,t){if(this.action=o,this.timestamp=t,this.type=dt,typeof o.type>`u`)throw new Error(`Actions may not have an undefined "type" property. Have you misspelled a constant?`)}};var Wt=class{constructor(){this.type=gi}};var Yt=class{constructor(o){this.timestamp=o,this.type=zn}};var Jt=class{constructor(o){this.timestamp=o,this.type=Ln}};var te=class{constructor(o){this.timestamp=o,this.type=Hn}};var ee=class{constructor(){this.type=$n}};var ne=class{constructor(o){this.id=o,this.type=Un}};var ie=class{constructor(o){this.index=o,this.type=Vn}};var oe=class{constructor(o){this.actionId=o,this.type=qn}};var re=class{constructor(o){this.nextLiftedState=o,this.type=le}};var ae=class{constructor(o){this.status=o,this.type=Gn}};var se=class{constructor(o){this.status=o,this.type=Zn}};var Rt=new A$1(`@ngrx/store-devtools Options`);var Rn=new A$1(`@ngrx/store-devtools Initial Config`);function Xn(){return null}var yi=`NgRx Store DevTools`;function _i(e){let o={maxAge:!1,monitor:Xn,actionSanitizer:void 0,stateSanitizer:void 0,actionCreators:void 0,name:yi,serialize:!1,logOnly:!1,autoPause:!1,trace:!1,traceLimit:75,features:{pause:!0,lock:!0,persist:!0,export:!0,import:`custom`,jump:!0,skip:!0,reorder:!0,dispatch:!0,test:!0},connectInZone:!1},t=typeof e==`function`?e():e,n=t.logOnly?{pause:!0,export:!0,test:!0}:!1,i=t.features||n||o.features;i.import===!0&&(i.import=`custom`);let r=Object.assign({},o,{features:i},t);if(r.maxAge&&r.maxAge<2)throw new Error(`Devtools 'maxAge' cannot be less than 2, got ${r.maxAge}`);return r}function Dn(e,o){return e.filter(t=>o.indexOf(t)<0)}function Kn(e){let{computedStates:o,currentStateIndex:t}=e;if(t>=o.length){let{state:i}=o[o.length-1];return i}let{state:n}=o[t];return n}function lt(e){return new tt(e,+Date.now())}function vi(e,o){return Object.keys(o).reduce((t,n)=>{let i=Number(n);return t[i]=Qn(e,o[i],i),t},{})}function Qn(e,o,t){return W$2(q$2({},o),{action:e(o.action,t)})}function Si(e,o){return o.map((t,n)=>({state:Wn(e,t.state,n),error:t.error}))}function Wn(e,o,t){return e(o,t)}function Yn(e){return e.predicate||e.actionsSafelist||e.actionsBlocklist}function xi(e,o,t,n){let i=[],r={},a=[];return e.stagedActionIds.forEach((d,f)=>{let s=e.actionsById[d];s&&(f&&de(e.computedStates[f],s,o,t,n)||(r[d]=s,i.push(d),a.push(e.computedStates[f])))}),W$2(q$2({},e),{stagedActionIds:i,actionsById:r,computedStates:a})}function de(e,o,t,n,i){let r=t&&!t(e,o.action),a=n&&!o.action.type.match(n.map(f=>On(f)).join(`|`)),d=i&&o.action.type.match(i.map(f=>On(f)).join(`|`));return r||a||d}function On(e){return e.replace(/[.*+?^${}()|[\]\\]/g,`\\$&`)}function Jn(e){return{ngZone:e?T$1(Se):null,connectInZone:e}}var Dt=(()=>{class e extends R$2{static{this.ɵfac=(()=>{let t;return function(i){return(t||(t=$m(e)))(i||e)}})()}static{this.ɵprov=oe$2({token:e,factory:e.ɵfac})}}return e})();var Tt={START:`START`,DISPATCH:`DISPATCH`,STOP:`STOP`,ACTION:`ACTION`};var ce=new A$1(`@ngrx/store-devtools Redux Devtools Extension`);function Ai(e){return typeof e==`object`&&e!==null&&!(`type`in e)&&typeof e.selected==`number`&&Array.isArray(e.args)}function Bn(e){let o=String(e),t=o.match(/^[^(]*\(([^)]*)\)/);if(!t){let n=o.match(/^\s*([^=\s(]+)\s*=>/);return n?[n[1]]:[]}return t[1].split(`,`).map(n=>n.replace(/^\s*\.{3}/,``).split(`=`)[0].trim()).filter(n=>n!==``)}function ki(e){return Array.isArray(e)?e.map(o=>({name:o.type||o.name||`anonymous`,func:o,args:Bn(o)})):Object.keys(e).map(o=>({name:o,func:e[o],args:Bn(e[o])}))}var jn=e=>e===``?void 0:(0,eval)(`(${e})`);var ti=(()=>{class e{constructor(t,n,i){this.config=n,this.dispatcher=i,this.zoneConfig=Jn(this.config.connectInZone),this.devtoolsExtension=t,this.actionCreatorDescriptors=n.actionCreators?ki(n.actionCreators):void 0,this.createActionStreams()}notify(t,n){if(this.devtoolsExtension)if(t.type===dt){if(n.isLocked||n.isPaused)return;let i=Kn(n);if(Yn(this.config)&&de(i,t,this.config.predicate,this.config.actionsSafelist,this.config.actionsBlocklist))return;let r=this.config.stateSanitizer?Wn(this.config.stateSanitizer,i,n.currentStateIndex):i,a=this.config.actionSanitizer?Qn(this.config.actionSanitizer,t,n.nextActionId):t;this.sendToReduxDevtools(()=>this.extensionConnection.send(a,r))}else{let i=W$2(q$2({},n),{stagedActionIds:n.stagedActionIds,actionsById:this.config.actionSanitizer?vi(this.config.actionSanitizer,n.actionsById):n.actionsById,computedStates:this.config.stateSanitizer?Si(this.config.stateSanitizer,n.computedStates):n.computedStates});this.sendToReduxDevtools(()=>this.devtoolsExtension.send(null,i,this.getExtensionConfig(this.config)))}}createChangesObservable(){return this.devtoolsExtension?new b(t=>{let n=this.zoneConfig.connectInZone?this.zoneConfig.ngZone.runOutsideAngular(()=>this.devtoolsExtension.connect(this.getExtensionConfig(this.config))):this.devtoolsExtension.connect(this.getExtensionConfig(this.config));return this.extensionConnection=n,n.init(),n.subscribe(i=>t.next(i)),n.unsubscribe}):He$1}createActionStreams(){let t=this.createChangesObservable().pipe(ss()),n=t.pipe(Hn$1(s=>s.type===Tt.START)),i=t.pipe(Hn$1(s=>s.type===Tt.STOP)),r=t.pipe(Hn$1(s=>s.type===Tt.DISPATCH),ae$1(s=>this.unwrapAction(s.payload)),og(s=>s.type===le?this.dispatcher.pipe(Hn$1(y=>y.type===rt$1),Uh(1e3),ig(1e3),ae$1(()=>s),rs(()=>es(s)),os(1)):es(s))),d=t.pipe(Hn$1(s=>s.type===Tt.ACTION),ae$1(s=>this.unwrapAction(s.payload))).pipe(Eg(i)),f=r.pipe(Eg(i));this.start$=n.pipe(Eg(i)),this.actions$=this.start$.pipe(jl(()=>d)),this.liftedActions$=this.start$.pipe(jl(()=>f))}unwrapAction(t){if(typeof t==`string`)return(0,eval)(`(${t})`);if(this.actionCreatorDescriptors&&Ai(t)){let n=this.actionCreatorDescriptors[t.selected];if(n){let i=t.args.map(jn);if(t.rest){let r=jn(t.rest);Array.isArray(r)&&i.push(...r)}return n.func(...i)}}return t}getExtensionConfig(t){let n={name:t.name,features:t.features,serialize:t.serialize,autoPause:t.autoPause??!1,trace:t.trace??!1,traceLimit:t.traceLimit??75};return t.maxAge!==!1&&(n.maxAge=t.maxAge),this.actionCreatorDescriptors&&(n.actionCreators=this.actionCreatorDescriptors),n}sendToReduxDevtools(t){try{t()}catch(n){console.warn(`@ngrx/store-devtools: something went wrong inside the redux devtools`,n)}}static{this.ɵfac=function(n){return new(n||e)(Ne(ce),Ne(Rt),Ne(Dt))}}static{this.ɵprov=oe$2({token:e,factory:e.ɵfac})}}return e})();var Nt={type:Te};var Ci={type:`@ngrx/store-devtools/recompute`};function ei(e,o,t,n,i){if(n)return{state:t,error:`Interrupted by an error up the chain`};let r=t,a;try{r=e(t,o)}catch(d){a=d.toString(),i.handleError(d)}return{state:r,error:a}}function Mt(e,o,t,n,i,r,a,d,f){if(o>=e.length&&e.length===r.length)return e;let s=e.slice(0,o),y=r.length-(f?1:0);for(let c=o;c<y;c++){let u=r[c],v=i[u].action,p=s[c-1],m=p?p.state:n,E=p?p.error:void 0,I=a.indexOf(u)>-1?p:ei(t,v,m,E,d);s.push(I)}return f&&s.push(e[e.length-1]),s}function Ei(e,o){return{monitorState:o(void 0,{}),nextActionId:1,actionsById:{0:lt(Nt)},stagedActionIds:[0],skippedActionIds:[],committedState:e,currentStateIndex:0,computedStates:[],isLocked:!1,isPaused:!1}}function Ii(e,o,t,n,i={}){return r=>(a,d)=>{let{monitorState:f,actionsById:s,nextActionId:y,stagedActionIds:c,skippedActionIds:u,committedState:v,currentStateIndex:p,computedStates:m,isLocked:E,isPaused:_}=a||o;a||(s=Object.create(s));function I(b){let g=b,j=c.slice(1,g+1);for(let k=0;k<j.length;k++)if(m[k+1].error){g=k,j=c.slice(1,g+1);break}else delete s[j[k]];u=u.filter(k=>j.indexOf(k)===-1),c=[0,...c.slice(g+1)],v=m[g].state,m=m.slice(g),p=p>g?p-g:0}function T(){s={0:lt(Nt)},y=1,c=[0],u=[],v=m[p].state,p=0,m=[]}let h=0;switch(d.type){case Gn:E=d.status,h=Infinity;break;case Zn:_=d.status,_?(c=[...c,y],s[y]=new tt({type:`@ngrx/devtools/pause`},+Date.now()),y++,h=c.length-1,m=m.concat(m[m.length-1]),p===c.length-2&&p++,h=Infinity):T();break;case zn:s={0:lt(Nt)},y=1,c=[0],u=[],v=e,p=0,m=[];break;case Hn:T();break;case Ln:s={0:lt(Nt)},y=1,c=[0],u=[],p=0,m=[];break;case Un:{let{id:b}=d;u.indexOf(b)===-1?u=[b,...u]:u=u.filter(j=>j!==b),h=c.indexOf(b);break}case bi:{let{start:b,end:g,active:j}=d,k=[];for(let Lt=b;Lt<g;Lt++)k.push(Lt);j?u=Dn(u,k):u=[...u,...k],h=c.indexOf(b);break}case Vn:p=d.index,h=Infinity;break;case qn:{let b=c.indexOf(d.actionId);b!==-1&&(p=b),h=Infinity;break}case $n:c=Dn(c,u),u=[],p=Math.min(p,c.length-1);break;case dt:{if(E)return a||o;if(_||a&&de(a.computedStates[p],d,i.predicate,i.actionsSafelist,i.actionsBlocklist)){let g=m[m.length-1];m=[...m.slice(0,-1),ei(r,d.action,g.state,g.error,t)],h=Infinity;break}i.maxAge&&c.length===i.maxAge&&I(1),p===c.length-1&&p++;let b=y++;s[b]=d,c=[...c,b],h=c.length-1;break}case le:({monitorState:f,actionsById:s,nextActionId:y,stagedActionIds:c,skippedActionIds:u,committedState:v,currentStateIndex:p,computedStates:m,isLocked:E,isPaused:_}=d.nextLiftedState);break;case Te:h=0,i.maxAge&&c.length>i.maxAge&&(m=Mt(m,h,r,v,s,c,u,t,_),I(c.length-i.maxAge),h=Infinity);break;case rt$1:if(m.filter(g=>g.error).length>0)h=0,i.maxAge&&c.length>i.maxAge&&(m=Mt(m,h,r,v,s,c,u,t,_),I(c.length-i.maxAge),h=Infinity);else{if(!_&&!E){p===c.length-1&&p++;let g=y++;s[g]=new tt(d,+Date.now()),c=[...c,g],h=c.length-1,m=Mt(m,h,r,v,s,c,u,t,_)}m=m.map(g=>W$2(q$2({},g),{state:r(g.state,Ci)})),p=c.length-1,i.maxAge&&c.length>i.maxAge&&I(c.length-i.maxAge),h=Infinity}break;default:h=Infinity;break}return m=Mt(m,h,r,v,s,c,u,t,_),f=n(f,d),{monitorState:f,actionsById:s,nextActionId:y,stagedActionIds:c,skippedActionIds:u,committedState:v,currentStateIndex:p,computedStates:m,isLocked:E,isPaused:_}}}var Pn=(()=>{class e{constructor(t,n,i,r,a,d,f,s){let y=Ei(f,s.monitor),c=Ii(f,y,d,s.monitor,s),u=ng(ng(n.asObservable().pipe(vg(1)),r.actions$).pipe(ae$1(lt)),t,r.liftedActions$).pipe(jn$1(Rh)),v=i.pipe(ae$1(c)),p=Jn(s.connectInZone),m=new Ln$1(1);this.liftedStateSubscription=u.pipe(wg(v),Fn(p),yg(({state:I},[T,h])=>{let b=h(I,T);return T.type!==dt&&Yn(s)&&(b=xi(b,s.predicate,s.actionsSafelist,s.actionsBlocklist)),r.notify(T,b),{state:b,action:T}},{state:y,action:null})).subscribe(({state:I,action:T})=>{if(m.next(I),T.type===dt){let h=T.action;a.next(h)}}),this.extensionStartSubscription=r.start$.pipe(Fn(p)).subscribe(()=>{this.refresh()});let E=m.asObservable(),_=E.pipe(ae$1(Kn));Object.defineProperty(_,"state",{value:ye(_,{manualCleanup:!0,requireSync:!0})}),this.dispatcher=t,this.liftedState=E,this.state=_}ngOnDestroy(){this.liftedStateSubscription.unsubscribe(),this.extensionStartSubscription.unsubscribe()}dispatch(t){this.dispatcher.next(t)}next(t){this.dispatcher.next(t)}error(t){}complete(){}performAction(t){this.dispatch(new tt(t,+Date.now()))}refresh(){this.dispatch(new Wt)}reset(){this.dispatch(new Yt(+Date.now()))}rollback(){this.dispatch(new Jt(+Date.now()))}commit(){this.dispatch(new te(+Date.now()))}sweep(){this.dispatch(new ee)}toggleAction(t){this.dispatch(new ne(t))}jumpToAction(t){this.dispatch(new oe(t))}jumpToState(t){this.dispatch(new ie(t))}importState(t){this.dispatch(new re(t))}lockChanges(t){this.dispatch(new ae(t))}pauseRecording(t){this.dispatch(new se(t))}static{this.ɵfac=function(n){return new(n||e)(Ne(Dt),Ne(R$2),Ne(E),Ne(ti),Ne(J$1),Ne(nt$1),Ne(W$3),Ne(Rt))}}static{this.ɵprov=oe$2({token:e,factory:e.ɵfac})}}return e})();function Fn({ngZone:e,connectInZone:o}){return t=>o?new b(n=>t.subscribe({next:i=>e.run(()=>n.next(i)),error:i=>e.run(()=>n.error(i)),complete:()=>e.run(()=>n.complete())})):t}var Ti=new A$1(`@ngrx/store-devtools Is Devtools Extension or Monitor Present`);function Mi(e,o){return!!e||o.monitor!==Xn}function Ni(){let e=`__REDUX_DEVTOOLS_EXTENSION__`;return typeof window==`object`&&typeof window[e]<`u`?window[e]:null}function Ri(e){return e.state}function ni(e={}){return To([ti,Dt,Pn,{provide:Rn,useValue:e},{provide:Ti,deps:[ce,Rt],useFactory:Mi},{provide:ce,useFactory:Ni},{provide:Rt,deps:[Rn],useFactory:_i},{provide:D$1,deps:[Pn],useFactory:Ri},{provide:w,useExisting:Dt}])}var ii=[{path:`auth`,loadChildren:()=>import(`./chunk-D6MI-CTP.js`).then(e=>e.authRoutes)},{path:``,pathMatch:`full`,canActivate:[p],loadComponent:()=>import(`./chunk-CyU2lVKV.js`).then(e=>e.HomeComponent)}];var H=class e{nextId=0;messages=Vo([]);notifyError(o){this.push(o,`error`)}notifyInfo(o){this.push(o,`info`)}dismiss(o){this.messages.update(t=>t.filter(n=>n.id!==o))}push(o,t){let n=this.nextId++;this.messages.update(i=>[...i,{id:n,text:o,level:t}]),setTimeout(()=>this.dismiss(n),6e3)}static ɵfac=function(t){return new(t||e)};static ɵprov=oe$2({token:e,factory:e.ɵfac,providedIn:`root`})};var Ot=null;var oi=(e,o)=>{let t=T$1(H),n$1=T$1(n),i=T$1(X$1);return o(e).pipe(rs(r=>{if(!(r instanceof ve))return ts(()=>r);if(r.status===401&&!e.url.includes(`/api/auth/login`)&&!e.url.includes(`/api/auth/refresh`))return Di(e,o,n$1,i);if(r.status!==401){let a=r.error&&typeof r.error==`object`&&`title`in r.error?String(r.error.title):`Request failed.`;t.notifyError(a)}return ts(()=>r)}))};function Di(e,o,t,n){return Ot||(Ot=t.refresh().pipe(Fl(1),Pl(()=>{Ot=null}))),Ot.pipe(jl(i=>{n.dispatch(In.tokenRefreshed({accessToken:i.accessToken,expiresAt:i.expiresAt,user:{id:i.user.id,fullName:i.user.fullName,email:i.user.email,role:i.user.role}}));return o(e.clone({setHeaders:{Authorization:`Bearer ${i.accessToken}`}}))}),rs(i=>(n.dispatch(In.logoutRequested()),ts(()=>i))))}var ri=(e,o)=>{let n=T$1(X$1).selectSignal(f.selectAccessToken)(),i=e;return n&&(i=i.clone({setHeaders:{Authorization:`Bearer ${n}`}})),i.url.startsWith(`/api/auth/`)&&(i=i.clone({withCredentials:!0})),o(i)};var Bt=class e{notifications=T$1(H);handleError(o){console.error(o),this.notifications.notifyError(`Something went wrong. Please try again.`)}static ɵfac=function(t){return new(t||e)};static ɵprov=oe$2({token:e,factory:e.ɵfac})};var jt=class e{actions$=T$1(bt);authService=T$1(n);router=T$1(Ke);login$=Ct(()=>this.actions$.pipe(Dt$1(In.loginSubmitted),jl(({email:o,password:t})=>this.authService.login({email:o,password:t}).pipe(ae$1(n=>In.loginSuccess({accessToken:n.accessToken,expiresAt:n.expiresAt,user:{id:n.user.id,fullName:n.user.fullName,email:n.user.email,role:n.user.role}})),rs(n=>es(In.loginFailure({error:n.error?.title??`Invalid credentials`})))))));logout$=Ct(()=>this.actions$.pipe(Dt$1(In.logoutRequested),jl(()=>this.authService.logout().pipe(ae$1(()=>In.logoutCompleted()),rs(()=>es(In.logoutCompleted()))))));logoutCompleted$=Ct(()=>this.actions$.pipe(Dt$1(In.logoutCompleted),Vl(()=>{this.router.navigateByUrl(`/auth/login`)})),{dispatch:!1});static ɵfac=function(t){return new(t||e)};static ɵprov=oe$2({token:e,factory:e.ɵfac})};var ai={providers:[em(),Nn(),Du$1(ii),Ps(Fs([ri,oi]),Ls({cookieName:`XSRF-TOKEN`,headerName:`X-XSRF-TOKEN`})),vn(),gn(f),wt(jt),ni({maxAge:25,logOnly:!GF()}),{provide:nt$1,useClass:Bt}]};function Oi(e,o){if(e&1){let t=HE();hi$1(0,`div`,1)(1,`button`,2),Op(`click`,function(){Eu(t);return Du(UE().action())}),ED(2),Ac()()}if(e&2){let t=UE();Sv(2),Pc(` `,t.data.action,` `)}}var Bi=[`label`];function ji(e,o){}var Pi=Math.pow(2,31)-1;var mt=class{_overlayRef;instance;containerInstance;_afterDismissed=new G$3;_afterOpened=new G$3;_onAction=new G$3;_durationTimeoutId;_dismissedByAction=!1;constructor(o,t){this._overlayRef=t,this.containerInstance=o,o._onExit.subscribe(()=>this._finishDismiss())}dismiss(){this._afterDismissed.closed||this.containerInstance.exit(),clearTimeout(this._durationTimeoutId)}dismissWithAction(){this._onAction.closed||(this._dismissedByAction=!0,this._onAction.next(),this._onAction.complete(),this.dismiss()),clearTimeout(this._durationTimeoutId)}closeWithAction(){this.dismissWithAction()}_dismissAfter(o){this._durationTimeoutId=setTimeout(()=>this.dismiss(),Math.min(o,Pi))}_open(){this._afterOpened.closed||(this._afterOpened.next(),this._afterOpened.complete())}_finishDismiss(){this._overlayRef.dispose(),this._onAction.closed||this._onAction.complete(),this._afterDismissed.next({dismissedByAction:this._dismissedByAction}),this._afterDismissed.complete(),this._dismissedByAction=!1}afterDismissed(){return this._afterDismissed}afterOpened(){return this.containerInstance._onEnter}onAction(){return this._onAction}};var si=new A$1(`MatSnackBarData`);var et=class{politeness=`polite`;announcementMessage=``;viewContainerRef;duration=0;panelClass;direction;data=null;horizontalPosition=`center`;verticalPosition=`bottom`};var Fi=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵdir=hE({type:e,selectors:[[``,`matSnackBarLabel`,``]],hostAttrs:[1,`mat-mdc-snack-bar-label`,`mdc-snackbar__label`]})}return e})();var zi=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵdir=hE({type:e,selectors:[[``,`matSnackBarActions`,``]],hostAttrs:[1,`mat-mdc-snack-bar-actions`,`mdc-snackbar__actions`]})}return e})();var Li=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵdir=hE({type:e,selectors:[[``,`matSnackBarAction`,``]],hostAttrs:[1,`mat-mdc-snack-bar-action`,`mdc-snackbar__action`]})}return e})();var ci=(()=>{class e{snackBarRef=T$1(mt);data=T$1(si);action(){this.snackBarRef.dismissWithAction()}get hasAction(){return!!this.data.action}static ɵfac=function(n){return new(n||e)};static ɵcmp=lE({type:e,selectors:[[`simple-snack-bar`]],hostAttrs:[1,`mat-mdc-simple-snack-bar`],exportAs:[`matSnackBar`],decls:3,vars:2,consts:[[`matSnackBarLabel`,``],[`matSnackBarActions`,``],[`matButton`,``,`matSnackBarAction`,``,3,`click`]],template:function(n,i){n&1&&(hi$1(0,`div`,0),ED(1),Ac(),xE(2,Oi,3,1,`div`,1)),n&2&&(Sv(),Pc(` `,i.data.message,`
`),Sv(),AE(i.hasAction?2:-1))},dependencies:[mt$1,Fi,zi,Li],styles:[`.mat-mdc-simple-snack-bar {
  display: flex;
}
.mat-mdc-simple-snack-bar .mat-mdc-snack-bar-label {
  max-height: 50vh;
  overflow: auto;
}
`],encapsulation:2})}return e})();var me=`_mat-snack-bar-enter`;var pe=`_mat-snack-bar-exit`;var Hi=(()=>{class e extends W{_ngZone=T$1(Se);_elementRef=T$1(vr);_changeDetectorRef=T$1(HF);_platform=T$1(v);_animationsDisabled=J$2();snackBarConfig=T$1(et);_document=T$1(rr);_trackedModals=new Set;_enterFallback;_exitFallback;_injector=T$1(he);_announceDelay=150;_announceTimeoutId;_destroyed=!1;_portalOutlet;_onAnnounce=new G$3;_onExit=new G$3;_onEnter=new G$3;_animationState=`void`;_live;_label;_role;_liveElementId=T$1(me$1).getId(`mat-snack-bar-container-live-`);constructor(){super();let t=this.snackBarConfig;t.politeness===`assertive`&&!t.announcementMessage?this._live=`assertive`:t.politeness===`off`?this._live=`off`:this._live=`polite`,this._platform.FIREFOX&&(this._live===`polite`&&(this._role=`status`),this._live===`assertive`&&(this._role=`alert`))}attachComponentPortal(t){this._assertNotAttached();let n=this._portalOutlet.attachComponentPortal(t);return this._afterPortalAttached(),n}attachTemplatePortal(t){this._assertNotAttached();let n=this._portalOutlet.attachTemplatePortal(t);return this._afterPortalAttached(),n}attachDomPortal=t=>{this._assertNotAttached();let n=this._portalOutlet.attachDomPortal(t);return this._afterPortalAttached(),n};onAnimationEnd(t){t===pe?this._completeExit():t===me&&(clearTimeout(this._enterFallback),this._ngZone.run(()=>{this._onEnter.next(),this._onEnter.complete()}))}enter(){this._destroyed||(this._animationState=`visible`,this._changeDetectorRef.markForCheck(),this._changeDetectorRef.detectChanges(),this._screenReaderAnnounce(),this._animationsDisabled?ov(()=>{this._ngZone.run(()=>queueMicrotask(()=>this.onAnimationEnd(me)))},{injector:this._injector}):(clearTimeout(this._enterFallback),this._enterFallback=setTimeout(()=>{this._elementRef.nativeElement.classList.add(`mat-snack-bar-fallback-visible`),this.onAnimationEnd(me)},200)))}exit(){return this._destroyed?es(void 0):(this._ngZone.run(()=>{this._animationState=`hidden`,this._changeDetectorRef.markForCheck(),this._elementRef.nativeElement.setAttribute(`mat-exit`,``),clearTimeout(this._announceTimeoutId),this._animationsDisabled?ov(()=>{this._ngZone.run(()=>queueMicrotask(()=>this.onAnimationEnd(pe)))},{injector:this._injector}):(clearTimeout(this._exitFallback),this._exitFallback=setTimeout(()=>this.onAnimationEnd(pe),200))}),this._onExit)}ngOnDestroy(){this._destroyed=!0,this._clearFromModals(),this._completeExit()}_completeExit(){clearTimeout(this._exitFallback),queueMicrotask(()=>{this._onExit.next(),this._onExit.complete()})}_afterPortalAttached(){let t=this._elementRef.nativeElement,n=this.snackBarConfig.panelClass;n&&(Array.isArray(n)?n.forEach(a=>t.classList.add(a)):t.classList.add(n)),this._exposeToModals();let i=this._label.nativeElement,r=`mdc-snackbar__label`;i.classList.toggle(r,!i.querySelector(`.${r}`))}_exposeToModals(){let t=this._liveElementId,n=this._document.querySelectorAll(`body > .cdk-overlay-container [aria-modal="true"]`);for(let i=0;i<n.length;i++){let r=n[i],a=r.getAttribute(`aria-owns`);this._trackedModals.add(r),a?a.indexOf(t)===-1&&r.setAttribute(`aria-owns`,a+` `+t):r.setAttribute(`aria-owns`,t)}}_clearFromModals(){this._trackedModals.forEach(t=>{let n=t.getAttribute(`aria-owns`);if(n){let i=n.replace(this._liveElementId,``).trim();i.length>0?t.setAttribute(`aria-owns`,i):t.removeAttribute(`aria-owns`)}}),this._trackedModals.clear()}_assertNotAttached(){this._portalOutlet.hasAttached()}_screenReaderAnnounce(){this._announceTimeoutId||this._ngZone.runOutsideAngular(()=>{this._announceTimeoutId=setTimeout(()=>{if(this._destroyed)return;let t=this._elementRef.nativeElement,n=t.querySelector(`[aria-hidden]`),i=t.querySelector(`[aria-live]`);if(n&&i){let r=null;this._platform.isBrowser&&document.activeElement instanceof HTMLElement&&n.contains(document.activeElement)&&(r=document.activeElement),n.removeAttribute(`aria-hidden`),i.appendChild(n),r?.focus(),this._onAnnounce.next(),this._onAnnounce.complete()}},this._announceDelay)})}static ɵfac=function(n){return new(n||e)};static ɵcmp=lE({type:e,selectors:[[`mat-snack-bar-container`]],viewQuery:function(n,i){if(n&1&&Pp(pe$1,7)(Bi,7),n&2){let r;QE(r=ZE())&&(i._portalOutlet=r.first),QE(r=ZE())&&(i._label=r.first)}},hostAttrs:[1,`mdc-snackbar`,`mat-mdc-snack-bar-container`],hostVars:6,hostBindings:function(n,i){n&1&&Op(`animationend`,function(a){return i.onAnimationEnd(a.animationName)})(`animationcancel`,function(a){return i.onAnimationEnd(a.animationName)}),n&2&&Bp(`mat-snack-bar-container-enter`,i._animationState===`visible`)(`mat-snack-bar-container-exit`,i._animationState===`hidden`)(`mat-snack-bar-container-animations-enabled`,!i._animationsDisabled)},features:[Ep],decls:6,vars:3,consts:[[`label`,``],[1,`mdc-snackbar__surface`,`mat-mdc-snackbar-surface`],[1,`mat-mdc-snack-bar-label`],[`aria-hidden`,`true`],[`cdkPortalOutlet`,``]],template:function(n,i){n&1&&(hi$1(0,`div`,1)(1,`div`,2,0)(3,`div`,3),wp(4,ji,0,0,`ng-template`,4),Ac(),Np(5,`div`),Ac()()),n&2&&(Sv(5),_p(`aria-live`,i._live)(`role`,i._role)(`id`,i._liveElementId))},dependencies:[pe$1],styles:[`@keyframes _mat-snack-bar-enter {
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
`],encapsulation:2,changeDetection:1})}return e})();var $i=new A$1(`mat-snack-bar-default-options`,{providedIn:`root`,factory:()=>new et});var Ui=(()=>{class e{_live=T$1(Mt$1);_injector=T$1(he);_breakpointObserver=T$1(It$1);_parentSnackBar=T$1(e,{optional:!0,skipSelf:!0});_defaultConfig=T$1($i);_animationsDisabled=J$2();_snackBarRefAtThisLevel=null;simpleSnackBarComponent=ci;snackBarContainerComponent=Hi;handsetCssClass=`mat-mdc-snack-bar-handset`;get _openedSnackBarRef(){let t=this._parentSnackBar;return t?t._openedSnackBarRef:this._snackBarRefAtThisLevel}set _openedSnackBarRef(t){this._parentSnackBar?this._parentSnackBar._openedSnackBarRef=t:this._snackBarRefAtThisLevel=t}openFromComponent(t,n){return this._attach(t,n)}openFromTemplate(t,n){return this._attach(t,n)}open(t,n=``,i){let r=q$2(q$2({},this._defaultConfig),i);return r.data={message:t,action:n},r.announcementMessage===t&&(r.announcementMessage=void 0),this.openFromComponent(this.simpleSnackBarComponent,r)}dismiss(){this._openedSnackBarRef&&this._openedSnackBarRef.dismiss()}ngOnDestroy(){this._snackBarRefAtThisLevel&&this._snackBarRefAtThisLevel.dismiss()}_attachSnackBarContainer(t,n){let i=n&&n.viewContainerRef&&n.viewContainerRef.injector,r=he.create({parent:i||this._injector,providers:[{provide:et,useValue:n}]}),a=new at(this.snackBarContainerComponent,n.viewContainerRef,r),d=t.attach(a);return d.instance.snackBarConfig=n,d.instance}_attach(t,n){let i=q$2(q$2(q$2({},new et),this._defaultConfig),n),r=this._createOverlay(i),a=this._attachSnackBarContainer(r,i),d=new mt(a,r);if(t instanceof fr){let f=new M(t,null,{$implicit:i.data,snackBarRef:d});d.instance=a.attachTemplatePortal(f)}else{let s=new at(t,void 0,this._createInjector(i,d));d.instance=a.attachComponentPortal(s).instance}return this._breakpointObserver.observe(Xn$1.HandsetPortrait).pipe(Eg(r.detachments())).subscribe(f=>{r.overlayElement.classList.toggle(this.handsetCssClass,f.matches)}),i.announcementMessage&&a._onAnnounce.subscribe(()=>{this._live.announce(i.announcementMessage,i.politeness)}),this._animateSnackBar(d,i),this._openedSnackBarRef=d,this._openedSnackBarRef}_animateSnackBar(t,n){t.afterDismissed().subscribe(()=>{this._openedSnackBarRef==t&&(this._openedSnackBarRef=null),n.announcementMessage&&this._live.clear()}),n.duration&&n.duration>0&&t.afterOpened().subscribe(()=>t._dismissAfter(n.duration)),this._openedSnackBarRef?(this._openedSnackBarRef.afterDismissed().subscribe(()=>{t.containerInstance.enter()}),this._openedSnackBarRef.dismiss()):t.containerInstance.enter()}_createOverlay(t){let n=new B;n.direction=t.direction;let i=$t(this._injector),r=t.direction===`rtl`,a=t.horizontalPosition===`left`||t.horizontalPosition===`start`&&!r||t.horizontalPosition===`end`&&r,d=!a&&t.horizontalPosition!==`center`;return a?i.left(`0`):d?i.right(`0`):i.centerHorizontally(),t.verticalPosition===`top`?i.top(`0`):i.bottom(`0`),n.positionStrategy=i,n.disableAnimations=this._animationsDisabled,gt(this._injector,n)}_createInjector(t,n){let i=t&&t.viewContainerRef&&t.viewContainerRef.injector;return he.create({parent:i||this._injector,providers:[{provide:mt,useValue:n},{provide:si,useValue:t.data}]})}static ɵfac=function(n){return new(n||e)};static ɵprov=yr({token:e,factory:e.ɵfac})}return e})();var li=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵmod=dE({type:e});static ɵinj=Gl({providers:[Ui],imports:[re$1,Vt,st,ci,at$2]})}return e})();var Vi=(e,o)=>o.id;function qi(e,o){if(e&1&&(Rc(0,`div`,1),ED(1),kc()),e&2){let t=o.$implicit;Bp(`notification--error`,t.level===`error`),Sv(),Pc(` `,t.text,` `)}}var Pt=class e{notifications=T$1(H);static ɵfac=function(t){return new(t||e)};static ɵcmp=lE({type:e,selectors:[[`app-notification`]],decls:2,vars:0,consts:[[1,`notification`,3,`notification--error`],[1,`notification`]],template:function(t,n){t&1&&kE(0,qi,2,3,`div`,0,Vi),t&2&&OE(n.notifications.messages())},dependencies:[li],styles:[`.notification[_ngcontent-%COMP%]{position:fixed;bottom:16px;right:16px;padding:12px 16px;border-radius:4px;background:#323232;color:#fff;margin-top:8px;z-index:1000}.notification--error[_ngcontent-%COMP%]{background:#b3261e}`]})};var Gi=[`*`,[[`mat-toolbar-row`]]];var Zi=[`*`,`mat-toolbar-row`];var Xi=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵdir=hE({type:e,selectors:[[`mat-toolbar-row`]],hostAttrs:[1,`mat-toolbar-row`],exportAs:[`matToolbarRow`]})}return e})();var mi=(()=>{class e{_elementRef=T$1(vr);_platform=T$1(v);_document=T$1(rr);color;_toolbarRows;ngAfterViewInit(){this._platform.isBrowser&&(this._checkToolbarMixedModes(),this._toolbarRows.changes.subscribe(()=>this._checkToolbarMixedModes()))}_checkToolbarMixedModes(){this._toolbarRows.length}static ɵfac=function(n){return new(n||e)};static ɵcmp=lE({type:e,selectors:[[`mat-toolbar`]],contentQueries:function(n,i,r){if(n&1&&Lp(r,Xi,5),n&2){let a;QE(a=ZE())&&(i._toolbarRows=a)}},hostAttrs:[1,`mat-toolbar`],hostVars:6,hostBindings:function(n,i){n&2&&(cD(i.color?`mat-`+i.color:``),Bp(`mat-toolbar-multiple-rows`,i._toolbarRows.length>0)(`mat-toolbar-single-row`,i._toolbarRows.length===0))},inputs:{color:`color`},exportAs:[`matToolbar`],ngContentSelectors:Zi,decls:2,vars:0,template:function(n,i){n&1&&(WE(Gi),GE(0),GE(1,1))},styles:[`.mat-toolbar {
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
`],encapsulation:2})}return e})();var pi=(()=>{class e{static ɵfac=function(n){return new(n||e)};static ɵmod=dE({type:e});static ɵinj=Gl({imports:[at$2]})}return e})();function Qi(e,o){if(e&1){let t=HE();hi$1(0,`mat-toolbar`,0)(1,`span`),ED(2,`ProjectManagementApp`),Ac(),Np(3,`span`,1),hi$1(4,`span`),ED(5),Ac(),hi$1(6,`button`,2),Op(`click`,function(){Eu(t);return Du(UE().logout())}),ED(7,`Log out`),Ac()()}if(e&2){let t=UE();Sv(5),Gp(t.user()?.fullName)}}var Ft=class e{store=T$1(X$1);user=this.store.selectSignal(f.selectUser);logout(){this.store.dispatch(In.logoutRequested())}static ɵfac=function(t){return new(t||e)};static ɵcmp=lE({type:e,selectors:[[`app-shell-header`]],decls:1,vars:1,consts:[[`color`,`primary`],[1,`spacer`],[`mat-button`,``,3,`click`]],template:function(t,n){t&1&&xE(0,Qi,8,1,`mat-toolbar`,0),t&2&&AE(n.user()?0:-1)},dependencies:[pi,mi,st,mt$1],styles:[`.spacer[_ngcontent-%COMP%]{flex:1 1 auto}`]})};ds(class e{static ɵfac=function(t){return new(t||e)};static ɵcmp=lE({type:e,selectors:[[`app-root`]],decls:3,vars:0,template:function(t,n){t&1&&Np(0,`app-shell-header`)(1,`router-outlet`)(2,`app-notification`)},dependencies:[Mr,Pt,Ft],encapsulation:2})},ai).catch(e=>console.error(e));export{Dt$1 as _,_t as a,pt as c,He as d,O$1 as f,z$1 as g,c as h,st as i,re$1 as l,Ze as m,f as n,dt$1 as o,Ue as p,mt$1 as r,ne$1 as s,p as t,B$1 as u,bt as v,n as y};