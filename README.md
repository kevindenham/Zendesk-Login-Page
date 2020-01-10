# Zendesk-Login-Page

I created this project while I was at home sick in bed one afternoon.  My organization had recently begun using Zendesk and one of the only features we found missing from the lower subscription level was OAuth SSO support.  A workaround existed in the form of JSON Web Token (JWT) Single Sign On but we had not ever implemented such a method for staff SSO.

I became interested, and using a mostly out-of-the-box Visual Studio template I began seeing what was needed to extend that template to include a JWT redirect to Zendesk after authentication with Office 365.  After heading down multiple dead ends, I found that access to the important information through the OAuth middleware was possible.  Though it required doing some hacky things, it worked.  The key element was creating an MVC project with authentication to Office 365, which took care of the messy problem of configuring the middleware.  I had previously tried to do this manually and ran into all sorts of issues.

In the end, it really required only modifying Startup.Auth.cs, and required less than 100 lines of custom code.  So for anyone interested in saving some money on Zendesk, you can bridge the gap between Microsoft's O365 Azure AD OAuth protocol and Zendesk's JWT consumer by just tweaking a single file, pulling out the user info you need, then assembling a JWT redirect to Zendesk.

I would later work with other third party vendors to set them up with custom OAuth Single Sign On solutions using this same method.  Thankfully,  Microsoft abstracts away the messier OAuth implementation and gives you access to an authentication token to do as you need.  
