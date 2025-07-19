window.spriteUtils = {
    extractSprites: async function (spriteSheetPath, spriteWidth, spriteHeight, columns, count) {
        const img = new Image();
        img.src = spriteSheetPath;
        await img.decode();

        const sprites = [];
        const canvas = document.createElement('canvas');
        canvas.width = spriteWidth;
        canvas.height = spriteHeight;
        const ctx = canvas.getContext('2d');

        for (let i = 0; i < count; i++) {
            const x = (i % columns) * spriteWidth;
            const y = Math.floor(i / columns) * spriteHeight;
            ctx.clearRect(0, 0, spriteWidth, spriteHeight);
            ctx.drawImage(img, x, y, spriteWidth, spriteHeight, 0, 0, spriteWidth, spriteHeight);
            sprites.push(canvas.toDataURL());
        }
        return sprites;
    }
};