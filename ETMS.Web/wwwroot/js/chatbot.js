// ── ETMS Chatbot ──

const botResponses = (msg) => {
    const m = msg.toLowerCase().trim();

    if (m.includes("hello") || m.includes("hi") || m.includes("hey"))
        return { text: "👋 Hello! How can I help you today?", chips: ["Transfer Request", "My Profile", "Settings", "Help"] };

    if (m.includes("transfer request") || m.includes("new transfer") || m.includes("create transfer") || m.includes("transfer"))
        return { text: "📋 To create a transfer request:<br>1. Go to <b>Dashboard</b><br>2. Click <b>'New Transfer Request'</b><br>3. Fill the form and submit.<br><br>Your manager gets notified automatically.", chips: ["Check Status", "Who approves?"] };

    if (m.includes("status") || m.includes("pending") || m.includes("my request"))
        return { text: "🔍 Go to <b>Dashboard → My Requests</b> to check your status. It will show as <b>Pending</b>, <b>Approved</b>, or <b>Rejected</b>.", chips: ["What if rejected?", "Timeline"] };

    if (m.includes("approve") || m.includes("who approve"))
        return { text: "✅ Approval chain:<br>1. <b>Reporting Manager</b><br>2. <b>HR / Branch Manager</b><br><br>You'll be notified at each step.", chips: ["How long?", "Main Menu"] };

    if (m.includes("how long") || m.includes("timeline"))
        return { text: "⏱️ Typically <b>3–7 working days</b>. Track progress in your dashboard anytime.", chips: ["Transfer Request", "Help"] };

    if (m.includes("profile") || m.includes("my profile"))
        return { text: "👤 Click your <b>avatar (top right)</b> → Select <b>'My Profile'</b>.<br>You can view and edit your employment info there.", chips: ["Edit Profile", "Change Password"] };

    if (m.includes("edit") || m.includes("update profile"))
        return { text: "✏️ On My Profile, click the <b>'Edit'</b> button top right. Make changes and click <b>'Save Changes'</b>.", chips: ["Change Password", "Settings"] };

    if (m.includes("password") || m.includes("change password"))
        return { text: "🔑 Go to <b>Settings → Account & Security → Change Password</b>.<br>Enter current password, then your new one.", chips: ["Forgot Password", "Settings"] };

    if (m.includes("forgot") || m.includes("reset"))
        return { text: "🔓 On the login page click <b>'Forgot Password?'</b>, enter your username and you'll receive an OTP to reset.", chips: ["Main Menu"] };

    if (m.includes("setting") || m.includes("notification"))
        return { text: "⚙️ In <b>Settings</b> you can manage:<br>• Password & 2FA<br>• Notifications<br>• Theme & Language<br>• Privacy", chips: ["Change Password", "My Profile"] };

    if (m.includes("rejected") || m.includes("what if"))
        return { text: "❌ If rejected, you'll get a notification with reason. You can edit & resubmit or contact HR.", chips: ["Transfer Request", "Contact HR"] };

    if (m.includes("contact") || m.includes("hr") || m.includes("help"))
        return { text: "📞 Contact HR:<br><b>HR Department</b><br>📧 hr@company.com<br>📞 +91-XXXX-XXXXXX", chips: ["Transfer Request", "My Profile"] };

    if (m.includes("logout") || m.includes("sign out"))
        return { text: "👋 Click your <b>avatar top right</b> → Select <b>'Logout'</b>.", chips: ["Main Menu"] };

    if (m.includes("main menu") || m.includes("menu") || m.includes("back"))
        return { text: "🏠 What would you like help with?", chips: ["Transfer Request", "My Profile", "Settings", "Contact HR"] };

    return { text: "🤔 I'm not sure about that. Here's what I can help with:", chips: ["Transfer Request", "My Profile", "Settings", "Contact HR"] };
};

function chatAddMessage(text, sender) {
    const box = document.getElementById('chat-messages');
    const div = document.createElement('div');
    div.className = `msg ${sender}`;
    div.innerHTML = text;
    box.appendChild(div);
    box.scrollTop = box.scrollHeight;
}

function chatShowTyping() {
    const box = document.getElementById('chat-messages');
    const div = document.createElement('div');
    div.className = 'typing-indicator';
    div.id = 'typing-indicator';
    div.innerHTML = '<div class="typing-dot"></div><div class="typing-dot"></div><div class="typing-dot"></div>';
    box.appendChild(div);
    box.scrollTop = box.scrollHeight;
}

function chatRemoveTyping() {
    const t = document.getElementById('typing-indicator');
    if (t) t.remove();
}

function chatAddChips(chips) {
    const box = document.getElementById('chat-messages');
    const div = document.createElement('div');
    div.className = 'quick-replies';
    div.id = 'quick-replies';
    chips.forEach(c => {
        const btn = document.createElement('button');
        btn.className = 'chip';
        btn.textContent = c;
        btn.onclick = () => { div.remove(); chatHandleSend(c); };
        div.appendChild(btn);
    });
    box.appendChild(div);
    box.scrollTop = box.scrollHeight;
}

function chatHandleSend(text) {
    const input = document.getElementById('chat-input');
    const msg = text || input.value.trim();
    if (!msg) return;
    input.value = '';

    const qr = document.getElementById('quick-replies');
    if (qr) qr.remove();

    chatAddMessage(msg, 'user');
    chatShowTyping();

    setTimeout(() => {
        chatRemoveTyping();
        const r = botResponses(msg);
        chatAddMessage(r.text, 'bot');
        if (r.chips && r.chips.length > 0) chatAddChips(r.chips);
    }, 750);
}

function toggleChat() {
    const win = document.getElementById('chat-window');
    const bubble = document.getElementById('chat-bubble');
    win.classList.toggle('open');

    if (win.classList.contains('open')) {
        document.getElementById('chat-input').focus();
        // hide unread badge
        const badge = document.getElementById('chat-unread');
        if (badge) badge.style.display = 'none';
    }
}

// Init on page load
document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('chat-input').addEventListener('keydown', function (e) {
        if (e.key === 'Enter') chatHandleSend();
    });
    // Show default chips after load
    setTimeout(() => chatAddChips(["Transfer Request", "My Profile", "Settings", "Help"]), 400);
});